// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Events.Suggestions;

namespace Cratis.Prologue.Workbench.Events.Accepted;

/// <summary>
/// An event of the accepted model.
/// </summary>
/// <param name="Key">The suggestion it came from.</param>
/// <param name="Name">The final name: the refined name, or the one a person chose.</param>
/// <param name="Mechanical">The name the heuristics gave it.</param>
/// <param name="Stream">The stream, which is the event source the event is appended to.</param>
/// <param name="Properties">The properties.</param>
/// <param name="Confidence">How strongly the evidence supports it.</param>
/// <param name="IsOpen">Whether nobody has accepted it yet. An open event is part of the model as a draft.</param>
public record AcceptedEvent(
    SuggestionKey Key,
    EventName Name,
    EventName Mechanical,
    StreamName Stream,
    IEnumerable<SuggestedProperty> Properties,
    Confidence Confidence,
    bool IsOpen);
