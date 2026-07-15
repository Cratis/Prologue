// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Interpreter.Contracts;

/// <summary>
/// Represents an event inferred for a slice — a past-tense fact derived from an observed database transaction that
/// followed a command.
/// </summary>
/// <param name="Name">The past-tense name of the event (for example <c>AuthorRegistered</c>).</param>
/// <param name="Properties">The inferred payload properties of the event.</param>
public record ExtractedEvent(string Name, IReadOnlyList<ExtractedProperty> Properties);
