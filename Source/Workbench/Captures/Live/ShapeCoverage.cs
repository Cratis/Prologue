// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.ReadModels;
using Cratis.Prologue.Workbench.Captures.Recording;
using Cratis.Prologue.Workbench.Common;
using Cratis.Prologue.Workbench.Decisions;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Captures.Live;

/// <summary>
/// The command shapes a prologue has captured and how often, so what has never been captured stands out.
/// </summary>
/// <param name="Id">The prologue.</param>
/// <param name="Shapes">The shapes seen, in the order they were first seen.</param>
[ReadModel]
public record ShapeCoverage(PrologueId Id, IEnumerable<ShapeCount> Shapes)
{
    /// <summary>
    /// Observes the coverage of a prologue.
    /// </summary>
    /// <param name="id">The prologue.</param>
    /// <param name="readModels">Observes the stored coverage.</param>
    /// <returns>The coverage, pushed again whenever a capture arrives.</returns>
    public static ISubject<ShapeCoverage> ObserveShapeCoverage(PrologueId id, IReadModels readModels) =>
        readModels.Observe<ShapeCoverage>(coverage => coverage.Id == id).FirstItem();

    /// <summary>
    /// Reduces a capture arriving. Captures without a command belong to no shape.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue's identity.</param>
    /// <returns>The new state.</returns>
    public static ShapeCoverage Reduce(CaptureRecorded @event, ShapeCoverage? current, EventContext context)
    {
        var coverage = current ?? new(new PrologueId(Guid.Parse(context.EventSourceId.Value)), []);
        if (@event.Http.Shape.IsNone)
        {
            return coverage;
        }

        var outcome = DecisionRules.Classify(@event.Http);
        var accepted = outcome == DecisionOutcome.Accepted ? 1 : 0;
        var refused = outcome == DecisionOutcome.Refused ? 1 : 0;

        var shapes = coverage.Shapes.ToList();
        var index = shapes.FindIndex(shape => shape.Shape == @event.Http.Shape);
        if (index < 0)
        {
            shapes.Add(new ShapeCount(@event.Http.Shape, 1, accepted, refused, @event.Scenario));
        }
        else
        {
            var seen = shapes[index];
            shapes[index] = seen with
            {
                Captures = seen.Captures + 1,
                Accepted = seen.Accepted + accepted,
                Refused = seen.Refused + refused,
                LastScenario = @event.Scenario
            };
        }

        return coverage with { Shapes = shapes };
    }
}
