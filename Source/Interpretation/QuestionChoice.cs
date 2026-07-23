// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Interpretation;

/// <summary>
/// Represents one choice offered by an <see cref="InterpreterQuestion"/>.
/// </summary>
/// <param name="Label">The short label of the choice — the value an answer carries when the choice is picked.</param>
/// <param name="Description">An optional one-line elaboration of what picking the choice means; empty when the label speaks for itself.</param>
public record QuestionChoice(string Label, string Description);
