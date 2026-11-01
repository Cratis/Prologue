// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Captures;

namespace Cratis.Prologue.Workbench.Rebuild;

/// <summary>
/// A slice being rebuilt: the legacy commands it replaces, how far it has come and how its decisions compare with the legacy system's.
/// </summary>
/// <param name="Name">The name of the slice.</param>
/// <param name="Shapes">The legacy command shapes the slice replaces.</param>
/// <param name="Stage">How far the slice has come.</param>
/// <param name="Matched">The mirrored commands both systems decided the same way.</param>
/// <param name="Mismatched">The mirrored commands the systems decided differently.</param>
/// <param name="Unavailable">The commands the new system could not answer; says nothing about its decisions.</param>
/// <param name="Skipped">The commands that were not mirrored.</param>
/// <param name="OpenDifferences">The differences nobody has resolved yet.</param>
public record RebuildSlice(
    RebuildSliceName Name,
    IEnumerable<CommandShape> Shapes,
    RebuildStage Stage,
    int Matched,
    int Mismatched,
    int Unavailable,
    int Skipped,
    IEnumerable<ObservationId> OpenDifferences)
{
    /// <summary>
    /// Gets the commands whose decisions were compared.
    /// </summary>
    /// <returns>The number compared.</returns>
    public int Compared() => Matched + Mismatched;

    /// <summary>
    /// Gets whether the slice replaces a command shape.
    /// </summary>
    /// <param name="shape">The shape.</param>
    /// <returns>True when the slice replaces it.</returns>
    public bool Replaces(CommandShape shape) => Shapes.Contains(shape);
}
