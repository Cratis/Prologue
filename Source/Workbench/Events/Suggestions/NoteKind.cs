// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Events.Suggestions;

/// <summary>
/// What a note on a suggestion is about.
/// </summary>
public enum NoteKind
{
    /// <summary>
    /// A business rule the evidence suggests, for example a refusal.
    /// </summary>
    Rule,

    /// <summary>
    /// A constraint the evidence suggests, for example a unique index.
    /// </summary>
    Constraint,

    /// <summary>
    /// Something worth knowing before accepting the suggestion.
    /// </summary>
    Observation
}
