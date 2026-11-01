// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Events.Suggestions;

/// <summary>
/// A suggestion with where it stands in the review.
/// </summary>
/// <param name="Suggestion">The suggestion as the last interpretation found it.</param>
/// <param name="Status">Where it stands in the review.</param>
/// <param name="Name">The name the event has now: the refined name until a person renames it.</param>
/// <param name="Renamed">Whether a person chose the name.</param>
/// <param name="Withdrawn">Whether the last interpretation no longer finds evidence for it.</param>
public record ReviewedSuggestion(SuggestedEvent Suggestion, SuggestionStatus Status, EventName Name, bool Renamed, bool Withdrawn);
