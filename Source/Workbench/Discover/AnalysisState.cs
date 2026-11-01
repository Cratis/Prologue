// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Discover;

/// <summary>
/// Where the analysis of a system or folder stands.
/// </summary>
public enum AnalysisState
{
    /// <summary>
    /// Never analyzed.
    /// </summary>
    NotAnalyzed,

    /// <summary>
    /// Waiting for its turn.
    /// </summary>
    Queued,

    /// <summary>
    /// Being read.
    /// </summary>
    Running,

    /// <summary>
    /// Read completely.
    /// </summary>
    Complete,

    /// <summary>
    /// Could not finish; what it found is kept.
    /// </summary>
    Failed
}
