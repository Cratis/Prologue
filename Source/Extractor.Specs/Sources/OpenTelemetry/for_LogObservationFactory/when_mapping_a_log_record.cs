// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Configuration;
using Cratis.Prologue.Extractor.Capturing;
using Google.Protobuf;
using OpenTelemetry.Proto.Collector.Logs.V1;
using OpenTelemetry.Proto.Common.V1;
using OpenTelemetry.Proto.Logs.V1;
using OpenTelemetry.Proto.Resource.V1;

namespace Cratis.Prologue.Extractor.Sources.OpenTelemetry.for_LogObservationFactory;

public class when_mapping_a_log_record : Specification
{
    LogObservationFactory _factory;
    ExportLogsServiceRequest _request;
    IReadOnlyList<Observation> _result;
    LogObserved _payload;

    void Establish()
    {
        _factory = new LogObservationFactory(new OpenTelemetryOptions { AttributeKeys = ["http.route"] });

        var record = new LogRecord
        {
            TraceId = ByteString.CopyFrom([.. Enumerable.Repeat((byte)0xAB, 16)]),
            SpanId = ByteString.CopyFrom([.. Enumerable.Repeat((byte)0x01, 8)]),
            SeverityText = "Warning",
            SeverityNumber = SeverityNumber.Warn,
            TimeUnixNano = 1_000_000_000,
            Body = new AnyValue { StringValue = "Author someone@example.com could not be deleted" },
        };
        record.Attributes.Add(new KeyValue { Key = "http.route", Value = new AnyValue { StringValue = "/api/authors/{id}" } });
        record.Attributes.Add(new KeyValue { Key = "author.email", Value = new AnyValue { StringValue = "someone@example.com" } });

        _request = new ExportLogsServiceRequest
        {
            ResourceLogs =
            {
                new ResourceLogs
                {
                    Resource = new Resource { Attributes = { new KeyValue { Key = "service.name", Value = new AnyValue { StringValue = "library" } } } },
                    ScopeLogs =
                    {
                        new ScopeLogs
                        {
                            Scope = new InstrumentationScope { Name = "Library.Controllers.AuthorsController" },
                            LogRecords = { record },
                        },
                    },
                },
            },
        };
    }

    void Because()
    {
        _result = [.. _factory.ToObservations(_request)];
        _payload = (LogObserved)_result[0].Payload;
    }

    [Fact] void should_produce_one_observation() => _result.Count.ShouldEqual(1);
    [Fact] void should_come_from_the_open_telemetry_source() => _result[0].Source.ShouldEqual(SourceKind.OpenTelemetry);
    [Fact] void should_hex_encode_the_trace_id() => _payload.TraceId.ShouldEqual("abababababababababababababababab");
    [Fact] void should_capture_the_severity_text() => _payload.SeverityText.ShouldEqual("Warning");
    [Fact] void should_capture_the_severity_number() => _payload.SeverityNumber.ShouldEqual((int)SeverityNumber.Warn);
    [Fact] void should_capture_the_service_name() => _payload.ServiceName.ShouldEqual("library");
    [Fact] void should_capture_the_scope_name() => _payload.ScopeName.ShouldEqual("Library.Controllers.AuthorsController");
    [Fact] void should_list_all_attribute_keys() => _payload.AttributeKeys.ShouldContainOnly(["http.route", "author.email"]);
    [Fact] void should_capture_allowlisted_attribute_values() => _payload.Attributes["http.route"].ShouldEqual("/api/authors/{id}");
    [Fact] void should_not_capture_values_outside_the_allowlist() => _payload.Attributes.ContainsKey("author.email").ShouldBeFalse();
}
#endif
