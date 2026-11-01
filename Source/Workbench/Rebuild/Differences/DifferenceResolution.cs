// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Rebuild.Differences;

/// <summary>
/// What was decided about a difference between the legacy system and the new one.
/// </summary>
public enum DifferenceResolution
{
    /// <summary>
    /// Nobody has decided yet; it blocks the cut over.
    /// </summary>
    Open,

    /// <summary>
    /// The new behavior is right; the difference is kept.
    /// </summary>
    Accepted,

    /// <summary>
    /// The new slice is to be changed to decide as the legacy system did.
    /// </summary>
    ToBeMatched
}
