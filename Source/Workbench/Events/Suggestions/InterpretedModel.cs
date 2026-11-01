// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Events.Suggestions;

/// <summary>
/// The structure the last interpretation found: the slices with their commands, read models and projections, the events as references to
/// suggestions. People review the events; everything else is what the interpreter and the language model decided.
/// </summary>
/// <param name="SystemName">The name of the system, as the language model derived it; empty when it did not.</param>
/// <param name="Slices">The slices.</param>
public record InterpretedModel(string SystemName, IEnumerable<ModelSlice> Slices)
{
    /// <summary>
    /// Gets the model of an interpretation that found nothing.
    /// </summary>
    public static readonly InterpretedModel Empty = new(string.Empty, []);
}
