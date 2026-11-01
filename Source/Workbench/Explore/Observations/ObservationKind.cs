// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Explore.Observations;

/// <summary>
/// What kind of thing an observation about a command shape points at.
/// </summary>
public enum ObservationKind
{
    /// <summary>
    /// The same route inserts on first use and updates afterwards.
    /// </summary>
    InsertsThenUpdates = 0,

    /// <summary>
    /// Some captures were refused and wrote nothing.
    /// </summary>
    AnswersWithoutWriting = 1,

    /// <summary>
    /// A table is written only in some of the runs that write.
    /// </summary>
    WritesInSomeRuns = 2
}
