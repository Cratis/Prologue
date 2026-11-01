// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Events.Suggestions;

/// <summary>
/// A note on a suggestion: a rule or constraint the evidence suggests, or something to know before accepting.
/// </summary>
/// <param name="Kind">What the note is about.</param>
/// <param name="Text">The note.</param>
public record SuggestionNote(NoteKind Kind, string Text);
