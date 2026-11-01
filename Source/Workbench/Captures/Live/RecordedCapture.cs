// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Captures;
using Cratis.Prologue.Workbench.Captures.Recording;
using Cratis.Prologue.Workbench.Decisions;
using Cratis.Prologue.Workbench.Scenarios;

namespace Cratis.Prologue.Workbench.Captures.Live;

/// <summary>
/// A capture as the live feed shows it.
/// </summary>
/// <param name="Capture">The capture.</param>
/// <param name="Occurred">When the capture happened.</param>
/// <param name="Service">The service the capture was correlated within.</param>
/// <param name="Http">The HTTP command, empty when the capture had none.</param>
/// <param name="Transactions">The database transactions correlated with the command.</param>
/// <param name="SpanCount">The number of spans observed.</param>
/// <param name="Spans">The first spans observed.</param>
/// <param name="TraceLinked">Whether the spans were joined by trace; otherwise by the time window.</param>
/// <param name="Scenario">The scenario running when the capture arrived.</param>
/// <param name="Outcome">Whether the system accepted or refused the command.</param>
public record RecordedCapture(
    CaptureId Capture,
    DateTimeOffset Occurred,
    ServiceName Service,
    RecordedCommand Http,
    IEnumerable<RecordedTransaction> Transactions,
    int SpanCount,
    IEnumerable<RecordedSpan> Spans,
    bool TraceLinked,
    ScenarioName Scenario,
    DecisionOutcome Outcome)
{
    /// <summary>
    /// Summarizes a capture that was recorded.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <returns>The capture as the feed shows it.</returns>
    public static RecordedCapture From(CaptureRecorded @event) => new(
        @event.Capture,
        @event.Occurred,
        @event.Service,
        @event.Http,
        @event.Transactions,
        @event.SpanCount,
        @event.Spans,
        @event.TraceLinked,
        @event.Scenario,
        DecisionRules.Classify(@event.Http));
}
