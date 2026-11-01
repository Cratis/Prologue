// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Captures;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Rebuild;

/// <summary>
/// The rules about slices being rebuilt, shared by the commands that change them.
/// </summary>
public static class RebuildRules
{
    /// <summary>
    /// Gets the slice with a name. Names are compared without regard to case.
    /// </summary>
    /// <param name="board">The board, or null when the prologue has none yet.</param>
    /// <param name="name">The name.</param>
    /// <returns>The slice, or null when there is none.</returns>
    public static RebuildSlice? Find(RebuildBoard? board, RebuildSliceName name) =>
        board?.Slices.FirstOrDefault(slice => string.Equals(slice.Name.Value, name.Value.Trim(), StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Normalizes a command shape typed or picked: the method upper-cased and the path as it is.
    /// </summary>
    /// <param name="shape">The shape as given.</param>
    /// <returns>The shape to keep, or null when it is not "METHOD /path".</returns>
    public static CommandShape? NormalizeShape(CommandShape shape)
    {
        var parts = shape.Value.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts.Length == 2 && parts[0].All(char.IsAsciiLetter) && parts[1].StartsWith('/')
            ? new CommandShape($"{parts[0].ToUpperInvariant()} {parts[1]}")
            : null;
    }

    /// <summary>
    /// Gets why a slice cannot be added, when it cannot.
    /// </summary>
    /// <param name="board">The board, or null when the prologue has none yet.</param>
    /// <param name="name">The name as typed.</param>
    /// <param name="shapes">The shapes as given.</param>
    /// <returns>The refusal, or null when the slice can be added.</returns>
    public static ValidationResult? RefuseAdd(RebuildBoard? board, RebuildSliceName name, IEnumerable<CommandShape> shapes)
    {
        var normalized = shapes.Select(NormalizeShape).ToList();
        if (string.IsNullOrWhiteSpace(name.Value))
        {
            return ValidationResult.Error("Name the slice first");
        }

        if (Find(board, name) is not null)
        {
            return ValidationResult.Error($"There is already a slice called \"{name.Value.Trim()}\"");
        }

        if (normalized.Count == 0)
        {
            return ValidationResult.Error("Pick the legacy command the slice replaces");
        }

        if (normalized.Contains(null))
        {
            return ValidationResult.Error("A legacy command is written as METHOD /path, such as POST /api/loans");
        }

        var taken = normalized.Select(shape => (Shape: shape!, Owner: board?.Slices.FirstOrDefault(slice => slice.Replaces(shape!)))).FirstOrDefault(pair => pair.Owner is not null);
        return taken.Owner is null ? null : ValidationResult.Error($"{taken.Shape.Value} is already replaced by {taken.Owner.Name.Value}");
    }

    /// <summary>
    /// Gets why a slice cannot move on from a stage, when it cannot. A slice moves one stage at a time.
    /// </summary>
    /// <param name="board">The board, or null when the prologue has none yet.</param>
    /// <param name="name">The name of the slice.</param>
    /// <param name="from">The stage the slice must be at.</param>
    /// <returns>The refusal, or null when the slice is at that stage.</returns>
    public static ValidationResult? RefuseMoveFrom(RebuildBoard? board, RebuildSliceName name, RebuildStage from) =>
        Find(board, name) switch
        {
            null => ValidationResult.Error($"There is no slice called \"{name.Value.Trim()}\""),
            { } slice when slice.Stage != from => ValidationResult.Error($"{slice.Name.Value} is {slice.Stage}; only a slice that is {from} can move on from there"),
            _ => null
        };

    /// <summary>
    /// Gets why a slice cannot be cut over, when it cannot: it must be shadowing, it must have been compared at least once and
    /// every difference found must be resolved.
    /// </summary>
    /// <param name="board">The board, or null when the prologue has none yet.</param>
    /// <param name="name">The name of the slice.</param>
    /// <returns>The refusal, or null when the slice can be cut over.</returns>
    public static ValidationResult? RefuseCutOver(RebuildBoard? board, RebuildSliceName name)
    {
        if (RefuseMoveFrom(board, name, RebuildStage.Shadowing) is { } wrongStage)
        {
            return wrongStage;
        }

        var slice = Find(board, name)!;
        var open = slice.OpenDifferences.Count();
        if (open > 0)
        {
            return ValidationResult.Error($"{slice.Name.Value} still has {open} unresolved {(open == 1 ? "difference" : "differences")}. Accept each or match the legacy behavior first");
        }

        return slice.Compared() == 0
            ? ValidationResult.Error($"{slice.Name.Value} has not been compared with the legacy system yet. Let requests pass through while it shadows")
            : null;
    }

    /// <summary>
    /// Gets the prologue an event belongs to.
    /// </summary>
    /// <param name="context">The context of the event.</param>
    /// <returns>The prologue.</returns>
    internal static PrologueId PrologueOf(EventContext context) => new(Guid.Parse(context.EventSourceId.Value));
}
