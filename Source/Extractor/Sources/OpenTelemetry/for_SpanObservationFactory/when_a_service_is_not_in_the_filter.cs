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

public class when_a_service_is_not_in_the_filter : Specification
{
    SpanObservationFactory _factory;
    ExportTraceServiceRequest _request;
    IReadOnlyList<Observation> _result;

    void Establish()
    {
        _factory = new SpanObservationFactory(new OpenTelemetryOptions { ServiceNames = ["billing"] });
        _request = new ExportTraceServiceRequest
        {
            ResourceSpans =
            {
                new ResourceSpans
                {
                    Resource = new Resource { Attributes = { new KeyValue { Key = "service.name", Value = new AnyValue { StringValue = "checkout" } } } },
                    ScopeSpans = { new ScopeSpans { Spans = { new Span { Name = "PlaceOrder" } } } },
                },
            },
        };
    }

    void Because() => _result = [.. _factory.ToObservations(_request)];

    [Fact] void should_not_capture_the_span() => _result.Count.ShouldEqual(0);
}
#endif
