// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using Library.Composition;

// The library system, captured. Everything the Prologue Extractor needs to observe a running system is wired here:
// the system itself sits behind the extractor's reverse proxy, exports its telemetry to the extractor's OTLP
// endpoint, and writes to a database the extractor reads the change log of. What the extractor correlates goes to
// the Receiver, which stores it in MongoDB.
//
//     browser / simulation ──▶ Extractor (proxy :8080) ──▶ Core ──▶ PostgreSQL or SQL Server
//                                   ▲                       │              │
//                    OTLP :4317/:4318                       └── telemetry ─┘
//                                   │
//                              Receiver ──▶ MongoDB
//
// Run it with:  aspire run                     (PostgreSQL, the default)
//               aspire run -- --database mssql (SQL Server)
var builder = DistributedApplication.CreateBuilder(args);

var database = LibraryDatabaseKind.From(builder.Configuration);

// The Prologue this run's captures belong to. Fixed so repeated runs accumulate against the same Prologue and can
// be interpreted together; override with --prologue-id to separate them.
var prologueId = builder.Configuration["prologue-id"] ?? LibraryComposition.DefaultPrologueId;

// ── Storage for the captures ────────────────────────────────────────────────────────────────────────────────────
var mongo = builder.AddMongoDB("mongo")
    .WithLifetime(ContainerLifetime.Persistent);

var captureStore = mongo.AddDatabase("prologue");

var receiver = builder.AddProject<Projects.Receiver>("receiver")

    // The Receiver carries no launch profile and no Kestrel configuration — it is normally a container, where
    // ASPNETCORE_URLS decides — so Aspire has nothing to derive an endpoint from and gives it none. Declaring one
    // is what makes receiver.GetEndpoint("http") below resolvable; without it the extractor dies at startup trying
    // to resolve an endpoint that does not exist, reported only as a bare FailedToStart.
    .WithHttpEndpoint()

    .WithEnvironment("Prologue__Mongo__ConnectionString", captureStore)
    .WithEnvironment("Prologue__Mongo__Database", "Prologue")
    .WithEnvironment("Prologue__Mongo__Collection", "captures")
    .WaitFor(mongo);

// ── The database the library runs on ────────────────────────────────────────────────────────────────────────────
// Each engine needs preparing before its changes can be read: PostgreSQL must run with logical replication (the
// extractor creates its own publication and slot), and SQL Server needs the Agent running for CDC to capture.
IResourceBuilder<IResourceWithConnectionString> libraryDatabase;
IResourceBuilder<IResource> databaseServer;

if (database.IsSqlServer)
{
    var sqlServer = builder.AddSqlServer("sqlserver")
        .WithEnvironment("MSSQL_AGENT_ENABLED", "true")
        .WithLifetime(ContainerLifetime.Persistent);

    libraryDatabase = sqlServer.AddDatabase("library", "LibraryDb");
    databaseServer = sqlServer;
}
else
{
    var postgres = builder.AddPostgres("postgres")
        .WithArgs("-c", "wal_level=logical", "-c", "max_replication_slots=10", "-c", "max_wal_senders=10")
        .WithLifetime(ContainerLifetime.Persistent);

    libraryDatabase = postgres.AddDatabase("library");
    databaseServer = postgres;
}

// ── The system being captured ───────────────────────────────────────────────────────────────────────────────────
var core = builder.AddProject<Projects.Core>("core")
    .WithReference(libraryDatabase)
    .WithEnvironment("Library__Database__Provider", database.Provider)

    // Core creates its schema and seeds during startup, so it must wait for the database itself to exist — not
    // merely for the server process to be up, which says nothing about whether the database can be connected to.
    .WaitFor(libraryDatabase)

    // Without an explicit health check Aspire calls the resource healthy as soon as the process is running, which
    // is well before it serves anything. Anything waiting on core — the tests, the frontends — would race it.
    .WithHttpHealthCheck("/health");

// ── The Prologue Extractor ──────────────────────────────────────────────────────────────────────────────────────
var extractor = builder.AddProject<Projects.Extractor>("extractor")

    // The extractor's endpoints come from the Kestrel section in its own appsettings.json — Proxy, OtlpHttp and
    // OtlpGrpc — which Aspire discovers, so nothing here may redeclare them. Aspire assigns the extractor a free
    // port to listen on, but publishes on Kestrel's: 8080, 4317 and 4318, which on a developer machine are
    // exactly the ports something else already holds (Docker Desktop, most often). The published address is then
    // dead while the extractor itself is perfectly healthy, and everything pointed at the proxy — both frontends,
    // the simulation — fails with a connection reset that looks like the extractor's fault.
    //
    // Nulling the published port lets Aspire pick a free one, as it does for every resource that does not
    // configure Kestrel. The conventional ports stay in appsettings.json for standalone and container runs, where
    // they are correct and namespaced.
    .WithEndpoint(LibraryComposition.ProxyEndpointName, endpoint => endpoint.Port = null)
    .WithEndpoint(LibraryComposition.OtlpHttpEndpointName, endpoint => endpoint.Port = null)
    .WithEndpoint(LibraryComposition.OtlpGrpcEndpointName, endpoint => endpoint.Port = null)
    .WithExternalHttpEndpoints()

    .WithEnvironment("Prologue__PrologueId", prologueId)

    // Correlated captures go to the Receiver rather than to files.
    .WithEnvironment("Prologue__Output__Kind", "Api")
    .WithEnvironment("Prologue__Output__Api__Endpoint", receiver.GetEndpoint("http"))

    // Everything the library system sends is worth capturing; nothing else is running here.
    .WithEnvironment("Prologue__OpenTelemetry__Enabled", "true")

    // The reverse proxy in front of the system being captured. The route is declared here rather than relying on
    // the extractor's own cratis-prologue.json being picked up — if it were not, the proxy would quietly forward
    // nothing and the capture would look empty for no visible reason.
    .WithEnvironment("ReverseProxy__Routes__monitored__ClusterId", "monitored")
    .WithEnvironment("ReverseProxy__Routes__monitored__Match__Path", "{**catch-all}")
    .WithEnvironment(
        "ReverseProxy__Clusters__monitored__Destinations__primary__Address",
        core.GetEndpoint(LibraryComposition.HttpEndpointName))

    .WithEnvironment(context =>
    {
        // Forward telemetry on to the Aspire dashboard after capturing it, so the dashboard still shows the
        // system's traces. Without an upstream the extractor is a terminal sink and the dashboard goes quiet.
        // The extractor forwards over OTLP/HTTP, so it needs the dashboard's HTTP endpoint, not its gRPC one.
        var dashboardOtlp =
            builder.Configuration["ASPIRE_DASHBOARD_OTLP_HTTP_ENDPOINT_URL"] ??
            builder.Configuration["DOTNET_DASHBOARD_OTLP_HTTP_ENDPOINT_URL"];

        if (!string.IsNullOrWhiteSpace(dashboardOtlp))
        {
            context.EnvironmentVariables["Prologue__OpenTelemetry__Upstream__Http"] = dashboardOtlp;
        }

        // The change source for whichever engine is running. Only the matching one is configured, so the other
        // never starts a reader.
        if (database.IsSqlServer)
        {
            context.EnvironmentVariables["Prologue__SqlServer__0__Name"] = "library";
            context.EnvironmentVariables["Prologue__SqlServer__0__ConnectionString"] = libraryDatabase.Resource.ConnectionStringExpression;
        }
        else
        {
            context.EnvironmentVariables["Prologue__Postgres__0__Name"] = "library";
            context.EnvironmentVariables["Prologue__Postgres__0__ConnectionString"] = libraryDatabase.Resource.ConnectionStringExpression;
        }
    })
    .WaitFor(receiver)
    .WaitFor(databaseServer)

    // The extractor proxies to core, so core's endpoint has to exist before the extractor can be given it. Without
    // this the extractor starts the moment the receiver is ready — which happens first — and dies resolving an
    // endpoint that has not been allocated yet, reported only as a bare FailedToStart.
    .WaitFor(core);

// The system's telemetry must reach the extractor rather than going straight to the dashboard, and its simulated
// load must travel through the extractor's proxy — traffic that bypasses the proxy is never captured.
core
    .WithEnvironment("OTEL_EXPORTER_OTLP_ENDPOINT", extractor.GetEndpoint(LibraryComposition.OtlpHttpEndpointName))
    .WithEnvironment("OTEL_EXPORTER_OTLP_PROTOCOL", "http/protobuf")
    .WithEnvironment("OTEL_SERVICE_NAME", "library")
    .WithEnvironment("Library__Simulation__BaseAddress", extractor.GetEndpoint(LibraryComposition.ProxyEndpointName))
    .WithEnvironment("Library__Simulation__DefaultTransactionCount", LibraryComposition.DefaultTransactionCount.ToString(CultureInfo.InvariantCulture))
    .WithSimulationCommands(LibraryComposition.DefaultTransactionCount);

// ── The React frontend ──────────────────────────────────────────────────────────────────────────────────────────
// Its API calls go through the extractor's proxy, so a single-page app's traffic is captured the same way the
// server-rendered one's is. The Razor frontend needs no resource of its own — Core serves it.
builder.AddViteApp("web", "../Web")
    .WithEnvironment("VITE_API_BASE_URL", extractor.GetEndpoint(LibraryComposition.ProxyEndpointName))
    .WithExternalHttpEndpoints()
    .WaitFor(core);

await builder.Build().RunAsync();
