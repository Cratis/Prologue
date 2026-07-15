// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using Cratis.Prologue.Configuration;
using Cratis.Prologue.Extractor.Capturing;
using Cratis.Prologue.Extractor.Sources.Http;
using Cratis.Prologue.Extractor.Sources.OpenTelemetry;
using Cratis.Prologue.Extractor.Sources.Postgres;
using Cratis.Prologue.Extractor.Sources.SqlServer;
using Yarp.ReverseProxy.Transforms.Builder;

// Force invariant culture for the backend
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);

// Capture configuration comes from a dedicated cratis-prologue.json file (path overridable through the
// PROLOGUE_CONFIG environment variable) — not appsettings.json. appsettings.json only carries hosting concerns.
builder.Configuration.AddJsonFile(
    PrologueConfigurationFile.ResolvePath(builder.Environment.ContentRootPath),
    optional: true,
    reloadOnChange: true);

builder.Services.Configure<PrologueOptions>(builder.Configuration.GetSection(PrologueOptions.SectionName));

// Capture pipeline
builder.Services.AddSingleton<IObservationChannel, ObservationChannel>();
builder.Services.AddSingleton<ICorrelator, TimeWindowCorrelator>();
builder.Services.AddHostedService<CorrelationWorker>();

// Output pipeline — captures are enqueued into a buffer and drained by a background worker so output I/O never
// blocks capture (like an async logging sink). The configured output is either the Prologue API or rolling JSON files.
var prologue = builder.Configuration.GetSection(PrologueOptions.SectionName).Get<PrologueOptions>() ?? new PrologueOptions();
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

// HTTP command source — reverse proxy with a command-capturing transform
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
builder.Services.AddSingleton<ITransformProvider, CommandCaptureTransform>();

// Database change sources — one hosted service per configured database
foreach (var sqlServer in prologue.SqlServer)
{
    builder.Services.AddSingleton<IHostedService>(serviceProvider => new SqlServerChangeSource(
        sqlServer,
        serviceProvider.GetRequiredService<IObservationChannel>(),
        serviceProvider.GetRequiredService<ILogger<SqlServerChangeSource>>()));
}

foreach (var postgres in prologue.Postgres)
{
    builder.Services.AddSingleton<IHostedService>(serviceProvider => new PostgresChangeSource(
        postgres,
        serviceProvider.GetRequiredService<IObservationChannel>(),
        serviceProvider.GetRequiredService<ILogger<PostgresChangeSource>>()));
}

// OpenTelemetry source — the engine proxies OTLP (HTTP + gRPC), capturing span metadata and forwarding to the
// upstream collector. Traces carry intent (commands) and the events they produce, correlated by trace id.
if (prologue.OpenTelemetry.Enabled)
{
    builder.Services.AddGrpc();
    builder.Services.AddSingleton(new SpanObservationFactory(prologue.OpenTelemetry));
    builder.Services.AddSingleton<GrpcTraceForwarder>();
    builder.Services.AddSingleton<OtlpHttpProxy>();
    builder.Services.AddHttpClient(OtlpHttpProxy.UpstreamClientName);
}

var app = builder.Build();

app.MapReverseProxy();

if (prologue.OpenTelemetry.Enabled)
{
    app.MapGrpcService<OtlpTraceGrpcService>();
    app.MapPost("/v1/traces", (HttpContext context, OtlpHttpProxy proxy) => proxy.HandleTraces(context));
    app.MapPost("/v1/metrics", (HttpContext context, OtlpHttpProxy proxy) => proxy.HandleMetrics(context));
    app.MapPost("/v1/logs", (HttpContext context, OtlpHttpProxy proxy) => proxy.HandleLogs(context));
}

await app.RunAsync();
