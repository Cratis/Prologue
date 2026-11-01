// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Explore.Asking;

/// <summary>
/// How asking about the captures ended.
/// </summary>
public enum AnswerStatus
{
    /// <summary>
    /// The agent answered and cited at least one captured command shape.
    /// </summary>
    Answered = 0,

    /// <summary>
    /// The agent could not be asked, for example because no AI provider is configured for it.
    /// </summary>
    Unavailable = 1,

    /// <summary>
    /// The agent answered but cited no captured command shape, so the answer is not shown.
    /// </summary>
    Uncited = 2,

    /// <summary>
    /// Nothing has been captured yet, so there is nothing to answer from.
    /// </summary>
    NothingCaptured = 3
}
