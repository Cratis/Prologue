// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using Cratis.Prologue.Configuration;
using Cratis.Prologue.Extractor.Capturing;
using Cratis.Prologue.Extractor.Shadowing;
using Cratis.Prologue.Extractor.Sources.Http;
using Cratis.Prologue.Extractor.Sources.OpenTelemetry;
using Cratis.Prologue.Extractor.Sources.Postgres;
using Cratis.Prologue.Extractor.Sources.SqlServer;
using Yarp.ReverseProxy.Transforms.Builder;

#if DEBUG
if (args.FirstOrDefault() == "--specifications")
{
    return await Xunit.Runner.InProc.SystemConsole.ConsoleRunner.Run(args[1..]);
}
#endif

// Force invariant culture for the backend
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);

// Capture configuration comes from a dedicated cratis-prologue.json file (path overridable through the
// PROLOGUE_CONFIG environment variable) — not appsettings.json. appsettings.json only carries hosting concerns.
// The file is the baseline; the environment overrides it, which is how a container or an Aspire composition
// configures a deployed extractor.
builder.Configuration.AddPrologueConfiguration(builder.Environment.ContentRootPath, reloadOnChange: true);

builder.Services.Configure<PrologueOptions>(builder.Configuration.GetSection(PrologueOptions.SectionName));

// Capture pipeline
builder.Services.AddSingleton<IObservationChannel, ObservationChannel>();
builder.Services.AddSingleton<ICorrelator, TimeWindowCorrelator>();
builder.Services.AddHostedService<CorrelationWorker>();

// Output pipeline — captures are enqueued into a buffer and drained by a background worker so output I/O never
// blocks capture (like an async logging sink). The configured output is either the Prologue API or rolling JSON files.
var prologue = builder.Configuration.GetSection(PrologueOptions.SectionName).Get<PrologueOptions>() ?? new PrologueOptions();

// The services captured from — the default service plus any named services. Every observation is attributed to one
// of them, and observations are only ever correlated within a service.
var services = CaptureServices.From(prologue);
builder.Services.AddSingleton(services);
builder.Services.AddSingleton<CaptureBuffer>();
builder.Services.AddSingleton<ICaptureStore, BufferedCaptureStore>();
builder.Services.AddHostedService<CaptureOutputWorker>();

if (prologue.Output.Kind == OutputKind.Json)
{
    builder.Services.AddSingleton<ICaptureOutput>(serviceProvider => new JsonFileCaptureOutput(
        prologue.Output.Json,
        serviceProvider.GetRequiredService<ILogger<JsonFileCaptureOutput>>()));
}
else
{
    builder.Services.AddHttpClient<ICaptureOutput, ApiCaptureOutput>(client =>
        client.BaseAddress = new Uri(prologue.Output.Api.Endpoint.TrimEnd('/') + "/"));
}

// Shadowing — commands that match a configured rule are mirrored to the new system after the legacy system answered, and
// the two decisions are compared and reported to the Receiver as parity. Without rules nothing here is registered.
var shadowing = builder.Services.AddShadowing(prologue);

// HTTP command source — reverse proxy with a command-capturing transform
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
builder.Services.AddSingleton<ITransformProvider, CommandCaptureTransform>();

// Database change sources — one hosted service per configured database, in every service. Each source prepares its
// own database for capture (enabling CDC, checking logical replication) so the system being captured never has to
// know Prologue is watching it, and captures the database's schema once as structural evidence when capture starts.
// Each source publishes through a channel that attributes its observations to the service the database belongs to.
foreach (var (service, sqlServer) in services.All.SelectMany(captureService => captureService.SqlServer.Select(sqlServer => (captureService.Name, sqlServer))))
{
    builder.Services.AddSingleton<IHostedService>(serviceProvider => new SqlServerChangeSource(
        sqlServer,
        new ServiceObservationChannel(serviceProvider.GetRequiredService<IObservationChannel>(), service),
        new SqlServerChangeCapturePreparer(
            sqlServer,
            serviceProvider.GetRequiredService<ILogger<SqlServerChangeCapturePreparer>>()),
        new SqlServerSchemaCapture(
            sqlServer,
            serviceProvider.GetRequiredService<ILogger<SqlServerSchemaCapture>>()),
        serviceProvider.GetRequiredService<ILogger<SqlServerChangeSource>>()));
}

foreach (var (service, postgres) in services.All.SelectMany(captureService => captureService.Postgres.Select(postgres => (captureService.Name, postgres))))
{
    builder.Services.AddSingleton<IHostedService>(serviceProvider => new PostgresChangeSource(
        postgres,
        new ServiceObservationChannel(serviceProvider.GetRequiredService<IObservationChannel>(), service),
        new PostgresReplicationPreparer(
            postgres,
            serviceProvider.GetRequiredService<ILogger<PostgresReplicationPreparer>>()),
        new PostgresSchemaCapture(
            postgres,
            serviceProvider.GetRequiredService<ILogger<PostgresSchemaCapture>>()),
        serviceProvider.GetRequiredService<ILogger<PostgresChangeSource>>()));
}

// OpenTelemetry source — the engine proxies OTLP (HTTP + gRPC) for all three signals, capturing metadata and
// forwarding to the upstream collector. Traces carry intent (commands) and the events they produce, correlated by
// trace id; logs attach to that same trace; metrics describe what the system measures about itself.
if (prologue.OpenTelemetry.Enabled)
{
    builder.Services.AddGrpc();
    builder.Services.AddSingleton(new SpanObservationFactory(prologue.OpenTelemetry, services));
    builder.Services.AddSingleton(new MetricObservationFactory(prologue.OpenTelemetry, services));
    builder.Services.AddSingleton(new LogObservationFactory(prologue.OpenTelemetry, services));
    builder.Services.AddSingleton<OtlpUpstreamChannel>();
    builder.Services.AddSingleton<GrpcTraceForwarder>();
    builder.Services.AddSingleton<GrpcMetricsForwarder>();
    builder.Services.AddSingleton<GrpcLogsForwarder>();
    builder.Services.AddSingleton<OtlpHttpProxy>();
    builder.Services.AddHttpClient(OtlpHttpProxy.UpstreamClientName);
}

var app = builder.Build();

if (shadowing.Any)
{
    app.MapReverseProxy(pipeline =>
    {
        pipeline.UseMiddleware<ShadowMiddleware>();
        pipeline.UseSessionAffinity();
        pipeline.UseLoadBalancing();
        pipeline.UsePassiveHealthChecks();
    });
}
else
{
    app.MapReverseProxy();
}

if (prologue.OpenTelemetry.Enabled)
{
    app.MapGrpcService<OtlpTraceGrpcService>();
    app.MapGrpcService<OtlpMetricsGrpcService>();
    app.MapGrpcService<OtlpLogsGrpcService>();
    app.MapPost(OtlpHttpProxy.TracesPath, (HttpContext context, OtlpHttpProxy proxy) => proxy.HandleTraces(context));
    app.MapPost(OtlpHttpProxy.MetricsPath, (HttpContext context, OtlpHttpProxy proxy) => proxy.HandleMetrics(context));
    app.MapPost(OtlpHttpProxy.LogsPath, (HttpContext context, OtlpHttpProxy proxy) => proxy.HandleLogs(context));
}

await app.RunAsync();
return 0;
