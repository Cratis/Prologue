// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Captures;
using Cratis.Prologue.Workbench.Prologues;
using Cratis.Prologue.Workbench.Scenarios;

namespace Cratis.Prologue.Workbench.Captures.Recording;

/// <summary>
/// Records a capture the Receiver stored. The record is a summary: the HTTP command, the tables and columns each database
/// transaction changed, and the spans. It never holds a row value.
/// </summary>
/// <param name="Prologue">The prologue the capture belongs to; <see cref="PrologueId.Unassigned"/> when it belongs to none.</param>
/// <param name="Capture">The capture.</param>
/// <param name="Occurred">When the capture happened.</param>
/// <param name="Service">The service the capture was correlated within.</param>
/// <param name="Http">The HTTP command, or <see cref="RecordedCommand.None"/> when the capture only holds database transactions.</param>
/// <param name="Transactions">The database transactions correlated with the command.</param>
/// <param name="SpanCount">The number of spans observed.</param>
/// <param name="Spans">The first spans observed, bounded by <see cref="CaptureMapping.MaximumSpans"/>.</param>
/// <param name="TraceLinked">Whether the spans were joined to the command by its trace rather than by time alone.</param>
[Command]
[ReadProtection(ReadProtectionMode.On)]
public record RecordCapture(
    PrologueId Prologue,
    CaptureId Capture,
    DateTimeOffset Occurred,
    ServiceName Service,
    RecordedCommand Http,
    IEnumerable<RecordedTransaction> Transactions,
    int SpanCount,
    IEnumerable<RecordedSpan> Spans,
    bool TraceLinked)
{
    /// <summary>
    /// Handles the command, labeling the capture with the scenario running when it arrives. The script is read under read
    /// protection so a scenario ending and a capture arriving cannot both decide on stale state.
    /// </summary>
    /// <param name="script">The scenario script of the prologue.</param>
    /// <returns>The capture recorded.</returns>
    public CaptureRecorded Handle(ScenarioScript? script) =>
        new(Capture, Occurred, Service, Http, Transactions, SpanCount, Spans, TraceLinked, ScenarioRules.Running(script));
}

/// <summary>
/// A capture arrived and was stored.
/// </summary>
/// <param name="Capture">The capture.</param>
/// <param name="Occurred">When the capture happened.</param>
/// <param name="Service">The service the capture was correlated within.</param>
/// <param name="Http">The HTTP command; <see cref="RecordedCommand.None"/> for database transactions with no command.</param>
/// <param name="Transactions">The database transactions correlated with the command.</param>
/// <param name="SpanCount">The number of spans observed.</param>
/// <param name="Spans">The first spans observed.</param>
/// <param name="TraceLinked">Whether the spans were joined to the command by its trace rather than by time alone.</param>
/// <param name="Scenario">The scenario running when the capture arrived, or <see cref="ScenarioName.None"/>.</param>
[EventType]
public record CaptureRecorded(
    CaptureId Capture,
    DateTimeOffset Occurred,
    ServiceName Service,
    RecordedCommand Http,
    IEnumerable<RecordedTransaction> Transactions,
    int SpanCount,
    IEnumerable<RecordedSpan> Spans,
    bool TraceLinked,
    ScenarioName Scenario);
