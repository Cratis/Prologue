// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Interpreter.Contracts;

/// <summary>
/// Represents the event model the interpreter extracted from a Prologue's captures — the full Module → Feature →
/// Slice structure, with the events, commands, read models, and projections inferred for each slice.
/// </summary>
/// <param name="PrologueId">The Prologue the extraction was performed for.</param>
/// <param name="Modules">The modules that make up the extracted event model.</param>
public record ExtractionResult(Guid PrologueId, IReadOnlyList<ExtractedModule> Modules)
{
    /// <summary>
    /// Represents an empty extraction result for a Prologue that yielded no captures.
    /// </summary>
    /// <param name="prologueId">The Prologue the result is for.</param>
    /// <returns>An <see cref="ExtractionResult"/> with no modules.</returns>
    public static ExtractionResult Empty(Guid prologueId) => new(prologueId, []);
}
