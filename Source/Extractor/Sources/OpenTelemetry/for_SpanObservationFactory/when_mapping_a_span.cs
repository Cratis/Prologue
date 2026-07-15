// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Configuration;
using Cratis.Prologue.Extractor.Capturing;
using Google.Protobuf;
using OpenTelemetry.Proto.Collector.Trace.V1;
using OpenTelemetry.Proto.Common.V1;
using OpenTelemetry.Proto.Resource.V1;
using OpenTelemetry.Proto.Trace.V1;

namespace Cratis.Prologue.Extractor.Sources.OpenTelemetry.for_SpanObservationFactory;

public class when_mapping_a_span : Specification
{
    SpanObservationFactory _factory;
    ExportTraceServiceRequest _request;
    IReadOnlyList<Observation> _result;
    TelemetryObserved _payload;

    void Establish()
    {
        _factory = new SpanObservationFactory(new OpenTelemetryOptions { AttributeKeys = ["command.type"] });

        var span = new Span
        {
            TraceId = ByteString.CopyFrom([.. Enumerable.Repeat((byte)0xAB, 16)]),
            SpanId = ByteString.CopyFrom([.. Enumerable.Repeat((byte)0x01, 8)]),
            Name = "PlaceOrder",
            Kind = Span.Types.SpanKind.Server,
            StartTimeUnixNano = 1_000_000_000,
            EndTimeUnixNano = 1_005_000_000,
            Status = new Status { Code = Status.Types.StatusCode.Ok },
        };
        span.Attributes.Add(new KeyValue { Key = "command.type", Value = new AnyValue { StringValue = "PlaceOrder" } });
        span.Attributes.Add(new KeyValue { Key = "db.statement", Value = new AnyValue { StringValue = "secret sql" } });

        _request = new ExportTraceServiceRequest
        {
            ResourceSpans =
            {
                new ResourceSpans
                {
                    Resource = new Resource { Attributes = { new KeyValue { Key = "service.name", Value = new AnyValue { StringValue = "checkout" } } } },
                    ScopeSpans = { new ScopeSpans { Spans = { span } } },
                },
            },
        };
    }

    void Because()
    {
        _result = [.. _factory.ToObservations(_request)];
        _payload = (TelemetryObserved)_result[0].Payload;
    }

    [Fact] void should_produce_one_observation() => _result.Count.ShouldEqual(1);
    [Fact] void should_capture_the_span_name() => _payload.Name.ShouldEqual("PlaceOrder");
    [Fact] void should_capture_the_service_name() => _payload.ServiceName.ShouldEqual("checkout");
    [Fact] void should_capture_the_span_kind() => _payload.Kind.ShouldEqual("Server");
    [Fact] void should_hex_encode_the_trace_id() => _payload.TraceId.ShouldEqual("abababababababababababababababab");
    [Fact] void should_compute_the_duration_in_milliseconds() => _payload.DurationMilliseconds.ShouldEqual(5);
    [Fact] void should_list_all_attribute_keys() => _payload.AttributeKeys.ShouldContainOnly(["command.type", "db.statement"]);
    [Fact] void should_capture_allowlisted_attribute_values() => _payload.Attributes["command.type"].ShouldEqual("PlaceOrder");
    [Fact] void should_not_capture_values_outside_the_allowlist() => _payload.Attributes.ContainsKey("db.statement").ShouldBeFalse();
}
#endif
