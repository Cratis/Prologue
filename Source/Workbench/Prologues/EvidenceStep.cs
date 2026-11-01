// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Prologues;

/// <summary>
/// The evidence steps a prologue can gather. Discover and Capture feed the model; at least one of them stays on.
/// </summary>
public enum EvidenceStep
{
    /// <summary>
    /// Analyze the systems' source code.
    /// </summary>
    Discover,

    /// <summary>
    /// Watch the running systems.
    /// </summary>
    Capture,

    /// <summary>
    /// Track the new system against the captured decisions.
    /// </summary>
    Rebuild
}
