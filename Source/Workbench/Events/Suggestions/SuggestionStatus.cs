// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Events.Suggestions;

/// <summary>
/// Where a suggestion stands in the review.
/// </summary>
public enum SuggestionStatus
{
    /// <summary>
    /// Nobody has decided yet.
    /// </summary>
    Suggested,

    /// <summary>
    /// A person accepted it into the model.
    /// </summary>
    Accepted,

    /// <summary>
    /// A person rejected it. It stays visible and can be restored.
    /// </summary>
    Rejected
}
