// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Configuration;

namespace Cratis.Prologue.Extractor.Capturing.for_CaptureServices.when_resolving;

/// <summary>
/// A configuration written before named services existed must capture exactly as it always did — everything
/// belongs to the one, default service.
/// </summary>
public class without_named_services : Specification
{
    CaptureServices _services;
    bool _telemetryCaptured;
    ServiceName _telemetryService;

    void Establish() => _services = CaptureServices.From(new PrologueOptions
    {
        SqlServer = [new SqlServerOptions { Name = "main" }],
        Postgres = [new PostgresOptions { Name = "reporting" }],
    });

    void Because() => _telemetryCaptured = _services.TryResolveTelemetry("checkout", out _telemetryService);

    [Fact] void should_have_only_the_default_service() => _services.All.Single().Name.ShouldEqual(ServiceName.Default);
    [Fact] void should_give_the_default_service_the_sql_server_sources() => _services.All[0].SqlServer.Single().Name.ShouldEqual("main");
    [Fact] void should_give_the_default_service_the_postgres_sources() => _services.All[0].Postgres.Single().Name.ShouldEqual("reporting");
    [Fact] void should_attribute_any_cluster_to_the_default_service() => _services.ForCluster("monitored").ShouldEqual(ServiceName.Default);
    [Fact] void should_capture_telemetry_from_any_service() => _telemetryCaptured.ShouldBeTrue();
    [Fact] void should_attribute_telemetry_to_the_default_service() => _telemetryService.ShouldEqual(ServiceName.Default);
}
#endif
