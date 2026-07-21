// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Configuration;
using Cratis.Prologue.Extractor.Capturing;
using OpenTelemetry.Proto.Collector.Metrics.V1;
using OpenTelemetry.Proto.Common.V1;
using OpenTelemetry.Proto.Metrics.V1;
using OpenTelemetry.Proto.Resource.V1;

namespace Cratis.Prologue.Extractor.Sources.OpenTelemetry.for_MetricObservationFactory;

public class when_mapping_a_metric : Specification
{
    MetricObservationFactory _factory;
    ExportMetricsServiceRequest _request;
    IReadOnlyList<Observation> _result;
    MetricObserved _payload;

    void Establish()
    {
        _factory = new MetricObservationFactory(new OpenTelemetryOptions { AttributeKeys = ["http.route"] });

        var first = new NumberDataPoint { AsDouble = 42, TimeUnixNano = 1_000_000_000 };
        first.Attributes.Add(new KeyValue { Key = "http.route", Value = new AnyValue { StringValue = "/api/authors" } });
        first.Attributes.Add(new KeyValue { Key = "customer.email", Value = new AnyValue { StringValue = "someone@example.com" } });

        var second = new NumberDataPoint { AsDouble = 7, TimeUnixNano = 1_000_000_000 };

        var metric = new Metric
        {
            Name = "http.server.request.duration",
            Unit = "ms",
            Sum = new Sum { DataPoints = { first, second } },
        };

        _request = new ExportMetricsServiceRequest
        {
            ResourceMetrics =
            {
                new ResourceMetrics
                {
                    Resource = new Resource { Attributes = { new KeyValue { Key = "service.name", Value = new AnyValue { StringValue = "library" } } } },
                    ScopeMetrics = { new ScopeMetrics { Metrics = { metric } } },
                },
            },
        };
    }

    void Because()
    {
        _result = [.. _factory.ToObservations(_request)];
        _payload = (MetricObserved)_result[0].Payload;
    }

    [Fact] void should_produce_one_observation() => _result.Count.ShouldEqual(1);
    [Fact] void should_come_from_the_open_telemetry_source() => _result[0].Source.ShouldEqual(SourceKind.OpenTelemetry);
    [Fact] void should_capture_the_metric_name() => _payload.Name.ShouldEqual("http.server.request.duration");
    [Fact] void should_capture_the_metric_kind() => _payload.Kind.ShouldEqual("Sum");
    [Fact] void should_capture_the_unit() => _payload.Unit.ShouldEqual("ms");
    [Fact] void should_capture_the_service_name() => _payload.ServiceName.ShouldEqual("library");
    [Fact] void should_count_the_data_points() => _payload.DataPointCount.ShouldEqual(2);
    [Fact] void should_list_all_attribute_keys() => _payload.AttributeKeys.ShouldContainOnly(["http.route", "customer.email"]);
    [Fact] void should_capture_allowlisted_attribute_values() => _payload.Attributes["http.route"].ShouldEqual("/api/authors");
    [Fact] void should_not_capture_values_outside_the_allowlist() => _payload.Attributes.ContainsKey("customer.email").ShouldBeFalse();
}
#endif
