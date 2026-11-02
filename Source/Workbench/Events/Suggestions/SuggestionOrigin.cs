// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Events.Suggestions;

/// <summary>
/// Where the evidence for a suggested event comes from.
/// </summary>
public enum SuggestionOrigin
{
    /// <summary>
    /// Captured commands and the database changes they made.
    /// </summary>
    Captured,

    /// <summary>
    /// A capability Discover mapped in the code whose command was never captured.
    /// </summary>
    CodeOnly,

    /// <summary>
    /// Database writes that arrived with no HTTP command, such as a nightly job.
    /// </summary>
    NoCommand
}
