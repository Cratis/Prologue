// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Interpretation;
using Cratis.Prologue.Interpreter.Contracts;
using Cratis.Prologue.Workbench.Explore.Scope;

namespace Cratis.Prologue.Workbench.Events.Interpreting;

/// <summary>
/// Builds the heuristic model the way the interpretation scope asks: shapes split by status are modeled one part at a time, so each response
/// status becomes a candidate of its own.
/// </summary>
/// <param name="inner">The builder that models one part of the captures.</param>
/// <param name="scope">The scope, or null when nothing was ever excluded or split.</param>
public class ScopedHeuristicModelBuilder(IBuildHeuristicModel inner, InterpretationScope? scope) : IBuildHeuristicModel
{
    /// <inheritdoc/>
    public ExtractionResult Build(Guid prologueId, IReadOnlyList<Capture> captures) =>
        ModelMerge.Combine(
            prologueId,
            ScopedCaptures.Partition(captures, scope).Select(part => ModelMerge.Suffixed(inner.Build(prologueId, part.Captures), part.SliceSuffix)));
}
