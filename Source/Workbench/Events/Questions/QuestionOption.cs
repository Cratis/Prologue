// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Events.Questions;

/// <summary>
/// One choice of a question.
/// </summary>
/// <param name="Label">The short label; the answer when the choice is picked.</param>
/// <param name="Description">What picking the choice means; empty when the label speaks for itself.</param>
public record QuestionOption(string Label, string Description);
