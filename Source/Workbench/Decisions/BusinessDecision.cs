// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Linq;
using Cratis.Chronicle.ReadModels;
using Cratis.Prologue.Workbench.Captures;
using Cratis.Prologue.Workbench.Captures.Live;
using Cratis.Prologue.Workbench.Common;
using Cratis.Prologue.Workbench.Prologues;
using Cratis.Prologue.Workbench.Scenarios;

namespace Cratis.Prologue.Workbench.Decisions;

/// <summary>
/// A decision the observed system made about a command, with the reason for it: the intent joined with the result.
/// </summary>
/// <param name="Capture">The capture the decision was observed in.</param>
/// <param name="Occurred">When it happened.</param>
/// <param name="Service">The service the capture was correlated within.</param>
/// <param name="Shape">The shape of the command.</param>
/// <param name="Path">The path of the command, as requested.</param>
/// <param name="Status">The status the command was answered with.</param>
/// <param name="Outcome">Whether the command was accepted or refused.</param>
/// <param name="Scenario">The scenario the capture was labeled with.</param>
/// <param name="Reason">Why the system decided what it did.</param>
[ReadModel]
public record BusinessDecision(
    CaptureId Capture,
    DateTimeOffset Occurred,
    ServiceName Service,
    CommandShape Shape,
    RequestPath Path,
    HttpStatus Status,
    DecisionOutcome Outcome,
    ScenarioName Scenario,
    DecisionReason Reason)
{
    /// <summary>
    /// Observes the latest decisions of a prologue, newest first, each with its reason. The decision points are joined in
    /// when they change, so a rule Discover finds later explains the decisions already observed.
    /// </summary>
    /// <param name="id">The prologue.</param>
    /// <param name="readModels">Observes the stored captures.</param>
    /// <param name="reasons">The decision points Discover found.</param>
    /// <returns>The decisions.</returns>
    public static ISubject<IEnumerable<BusinessDecision>> ObserveBusinessDecisions(PrologueId id, IReadModels readModels, IDecisionReasons reasons)
    {
        var decisions = new ReplaySubject<IEnumerable<BusinessDecision>>(1);
        readModels.Observe<RecentCaptures>(recent => recent.Id == id).FirstItem()
            .CombineLatest(reasons.Observe(id), From)
            .Subscribe(decisions);
        return decisions;
    }

    /// <summary>
    /// Turns captures into decisions. Captures that hold no decision are left out.
    /// </summary>
    /// <param name="recent">The latest captures.</param>
    /// <param name="points">The decision points found in the code.</param>
    /// <returns>The decisions, in the order of the captures.</returns>
    public static IEnumerable<BusinessDecision> From(RecentCaptures recent, IEnumerable<DecisionPoint> points)
    {
        var known = points.ToList();
        return
        [
            .. recent.Captures
                .Where(capture => capture.Outcome != DecisionOutcome.NotDecided)
                .Select(capture => new BusinessDecision(
                    capture.Capture,
                    capture.Occurred,
                    capture.Service,
                    capture.Http.Shape,
                    capture.Http.Path,
                    capture.Http.Status,
                    capture.Outcome,
                    capture.Scenario,
                    DecisionReasoning.Select(capture, known)))
        ];
    }
}
