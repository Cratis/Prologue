// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Extractor.Capturing.for_JsonFileCaptureOutput;

/// <summary>
/// Capture files written before named services existed carry no service on their lines. They must still read back,
/// as captures of the default service.
/// </summary>
public class when_reading_a_capture_file_written_before_services : Specification
{
    const string Line = """{"captureId":"6f1c2c1e-6a35-4f69-9d2c-4a3b2a1c0d9e","occurred":"2026-01-01T00:00:00+00:00","source":"http","payload":{"type":"http-command","method":"POST","path":"/orders","statusCode":201,"traceId":""}}""";

    IReadOnlyList<Capture> _result = [];

    void Because() => _result = CaptureFiles.ToCaptures([Line]);

    [Fact] void should_read_the_capture() => _result.Count.ShouldEqual(1);
    [Fact] void should_attribute_the_capture_to_the_default_service() => _result[0].Service.ShouldEqual(ServiceName.Default);
    [Fact] void should_attribute_the_observation_to_the_default_service() => _result[0].Entries[0].Service.ShouldEqual(ServiceName.Default);
}
#endif
