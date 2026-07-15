// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Contracts;
using Cratis.Prologue.Interpreter.Contracts;

namespace Cratis.Prologue.Interpreter;

/// <summary>
/// Represents an <see cref="IRefineExtraction"/> that returns the provisional model unchanged — the default used
/// when no language model is configured, and in specs so the deterministic structure can be asserted in isolation.
/// </summary>
public class PassthroughExtractionRefiner : IRefineExtraction
{
    /// <inheritdoc/>
    public Task<ExtractionResult> Refine(ExtractionResult model, IReadOnlyList<Capture> captures, CancellationToken cancellationToken = default) =>
        Task.FromResult(model);
}
