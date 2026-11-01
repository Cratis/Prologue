// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Events.Suggestions;

/// <summary>
/// An interpretation found a different set of suggested events than the one before. The set replaces what was suggested: suggestions
/// that are new join it, ones that changed are updated and keep what people decided, and ones that are gone are withdrawn.
/// </summary>
/// <param name="Suggestions">Every event the interpretation suggests.</param>
[EventType]
public record EventSuggestionsInterpreted(IEnumerable<SuggestedEvent> Suggestions);
