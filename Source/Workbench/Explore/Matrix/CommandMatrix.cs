// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.ReadModels;
using Cratis.Prologue.Workbench.Captures.Recording;
using Cratis.Prologue.Workbench.Common;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Explore.Matrix;

/// <summary>
/// The command by table matrix of a prologue: for every command shape, how often it was captured, how it was answered and which
/// tables it wrote, kept up to date as each capture arrives so nothing is counted when the page opens.
/// </summary>
/// <param name="Id">The prologue.</param>
/// <param name="Shapes">The shapes seen, in the order they were first seen.</param>
[ReadModel]
public record CommandMatrix(PrologueId Id, IEnumerable<ShapeProfile> Shapes)
{
    /// <summary>
    /// Observes the matrix of a prologue.
    /// </summary>
    /// <param name="id">The prologue.</param>
    /// <param name="readModels">Observes the stored matrix.</param>
    /// <returns>The matrix, pushed again whenever a capture arrives.</returns>
    public static ISubject<CommandMatrix> ObserveCommandMatrix(PrologueId id, IReadModels readModels) =>
        readModels.Observe<CommandMatrix>(matrix => matrix.Id == id).FirstItem();

    /// <summary>
    /// Reduces a capture arriving. Captures without a command belong to no shape.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue's identity.</param>
    /// <returns>The new state.</returns>
    public static CommandMatrix Reduce(CaptureRecorded @event, CommandMatrix? current, EventContext context)
    {
        var matrix = current ?? new(new PrologueId(Guid.Parse(context.EventSourceId.Value)), []);
        if (@event.Http.Shape.IsNone)
        {
            return matrix;
        }

        var shapes = matrix.Shapes.ToList();
        var index = shapes.FindIndex(profile => profile.Shape == @event.Http.Shape);
        var folded = ShapeProfiles.Fold(index < 0 ? null : shapes[index], @event);
        if (index < 0)
        {
            shapes.Add(folded);
        }
        else
        {
            shapes[index] = folded;
        }

        return matrix with { Shapes = shapes };
    }
}
