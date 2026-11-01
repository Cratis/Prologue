// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG

namespace Cratis.Prologue.Workbench.Captures.Recording.for_CaptureMapping;

public class when_spans_have_another_trace : Specification
{
    RecordCapture _command;

    void Because()
    {
        var at = DateTimeOffset.UtcNow;
        var spans = Enumerable.Range(0, 40)
            .Select(index => new Observation(SourceKind.OpenTelemetry, at, new TelemetryObserved("other", $"s{index}", string.Empty, "span", "Server", "Lending", 0, 5, [], new Dictionary<string, string>())));
        var capture = new Capture(
            Guid.NewGuid(),
            at,
            [new(SourceKind.Http, at, new HttpCommandObserved("POST", "/api/loans", 201, "trace-1")), .. spans]);

        _command = CaptureMapping.ToCommand(capture);
    }

    [Fact] void should_not_be_trace_linked() => _command.TraceLinked.ShouldBeFalse();
    [Fact] void should_count_every_span() => _command.SpanCount.ShouldEqual(40);
    [Fact] void should_keep_a_bounded_number_of_spans() => _command.Spans.Count().ShouldEqual(CaptureMapping.MaximumSpans);
}
#endif
