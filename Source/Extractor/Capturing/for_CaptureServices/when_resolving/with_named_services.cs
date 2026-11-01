// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Configuration;

namespace Cratis.Prologue.Extractor.Capturing.for_CaptureServices.when_resolving;

public class with_named_services : Specification
{
    static readonly ServiceName _orders = "orders";
    static readonly ServiceName _billing = "billing";

    CaptureServices _services;

    void Establish() => _services = CaptureServices.From(new PrologueOptions
    {
        SqlServer = [new SqlServerOptions { Name = "legacy" }],
        OpenTelemetry = new OpenTelemetryOptions { ServiceNames = ["gateway"] },
        Services = new Dictionary<string, ServiceOptions>
        {
            ["orders"] = new() { SqlServer = [new SqlServerOptions { Name = "orders-db" }] },
            ["billing"] = new()
            {
                Postgres = [new PostgresOptions { Name = "billing-db" }],
                Clusters = ["billing-api", "billing-admin"],
                TelemetryServiceNames = ["Billing.Api"],
            },
        },
    });

    [Fact] void should_list_the_default_service_first() => _services.All[0].Name.ShouldEqual(ServiceName.Default);
    [Fact] void should_list_every_named_service() => _services.All.Count.ShouldEqual(3);
    [Fact] void should_keep_the_top_level_databases_on_the_default_service() => _services.All[0].SqlServer.Single().Name.ShouldEqual("legacy");
    [Fact] void should_give_a_named_service_its_own_databases() => _services.All.Single(service => service.Name == _billing).Postgres.Single().Name.ShouldEqual("billing-db");
    [Fact] void should_attribute_the_cluster_named_after_a_service_to_it() => _services.ForCluster("orders").ShouldEqual(_orders);
    [Fact] void should_attribute_explicitly_claimed_clusters() => _services.ForCluster("billing-admin").ShouldEqual(_billing);
    [Fact] void should_not_claim_the_service_name_as_a_cluster_when_clusters_are_listed() => _services.ForCluster("billing").ShouldEqual(ServiceName.Default);
    [Fact] void should_attribute_an_unclaimed_cluster_to_the_default_service() => _services.ForCluster("monitored").ShouldEqual(ServiceName.Default);
    [Fact] void should_attribute_telemetry_named_after_a_service_to_it() => Resolve("orders").ShouldEqual(_orders);
    [Fact] void should_attribute_explicitly_claimed_telemetry_ignoring_case() => Resolve("billing.api").ShouldEqual(_billing);
    [Fact] void should_attribute_unclaimed_telemetry_passing_the_filter_to_the_default_service() => Resolve("gateway").ShouldEqual(ServiceName.Default);
    [Fact] void should_not_capture_unclaimed_telemetry_outside_the_filter() => _services.TryResolveTelemetry("collector", out _).ShouldBeFalse();

    ServiceName Resolve(string telemetryServiceName)
    {
        _services.TryResolveTelemetry(telemetryServiceName, out var service).ShouldBeTrue();
        return service;
    }
}
#endif
