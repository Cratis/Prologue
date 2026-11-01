// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Captures;

namespace Cratis.Prologue.Workbench.Events.Suggestions;

/// <summary>
/// A command shape whose captures evidence a suggestion.
/// </summary>
/// <param name="Shape">The command shape, for example POST /api/loans.</param>
/// <param name="Captures">How many captures of the shape evidence the suggestion.</param>
public record ShapeEvidence(CommandShape Shape, int Captures);
