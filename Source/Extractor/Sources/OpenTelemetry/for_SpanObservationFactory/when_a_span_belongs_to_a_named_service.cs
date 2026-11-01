// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Configuration;
using Cratis.Prologue.Extractor.Capturing;
using OpenTelemetry.Proto.Collector.Trace.V1;
using OpenTelemetry.Proto.Common.V1;
using OpenTelemetry.Proto.Resource.V1;
using OpenTelemetry.Proto.Trace.V1;

namespace Cratis.Prologue.Extractor.Sources.OpenTelemetry.for_SpanObservationFactory;

public class when_a_span_belongs_to_a_named_service : Specification
{
    SpanObservationFactory _factory;
    ExportTraceServiceRequest _request;
    IReadOnlyList<Observation> _result;

    void Establish()
    {
        var options = new OpenTelemetryOptions { ServiceNames = ["billing"] };
        _factory = new SpanObservationFactory(options, CaptureServices.From(new PrologueOptions
        {
            OpenTelemetry = options,
            Services = new Dictionary<string, ServiceOptions> { ["orders"] = new() { TelemetryServiceNames = ["Orders.Api"] } },
        }));
        _request = new ExportTraceServiceRequest
        {
            ResourceSpans =
            {
                new ResourceSpans
                {
                    Resource = new Resource { Attributes = { new KeyValue { Key = "service.name", Value = new AnyValue { StringValue = "Orders.Api" } } } },
                    ScopeSpans = { new ScopeSpans { Spans = { new Span { Name = "PlaceOrder" } } } },
                },
            },
        };
    }

    void Because() => _result = [.. _factory.ToObservations(_request)];

    [Fact] void should_capture_the_span_even_though_the_default_filter_excludes_it() => _result.Count.ShouldEqual(1);
    [Fact] void should_attribute_the_span_to_the_named_service() => _result[0].Service.ShouldEqual(new ServiceName("orders"));
}
#endif
