// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Discover;

namespace Cratis.Prologue.Workbench.Events.Suggestions;

/// <summary>
/// An event the interpreter suggests, with everything a person needs to judge it.
/// </summary>
/// <param name="Key">The identity that survives interpretations.</param>
/// <param name="Mechanical">The name the heuristics gave it, from the table and the operation.</param>
/// <param name="Refined">The name the language model refined it to; the mechanical name when none was refined.</param>
/// <param name="Module">The module the event belongs to.</param>
/// <param name="Stream">The stream the event belongs to.</param>
/// <param name="Properties">The properties the event carries.</param>
/// <param name="Confidence">How strongly the evidence supports it.</param>
/// <param name="Evidence">What the suggestion rests on.</param>
/// <param name="Notes">The rules and constraints the evidence suggests.</param>
/// <param name="Origin">Where the evidence comes from; captured commands unless the suggestion rests on code alone or on writes with no command.</param>
public record SuggestedEvent(
    SuggestionKey Key,
    EventName Mechanical,
    EventName Refined,
    ModuleName Module,
    StreamName Stream,
    IEnumerable<SuggestedProperty> Properties,
    Confidence Confidence,
    SuggestionEvidence Evidence,
    IEnumerable<SuggestionNote> Notes,
    SuggestionOrigin Origin = SuggestionOrigin.Captured);
