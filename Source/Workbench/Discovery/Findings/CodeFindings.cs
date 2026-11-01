// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.ReadModels;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Discovery.Findings;

/// <summary>
/// What the analyses of a prologue's code found, for every system and folder: the capabilities, the decision points, the
/// dependencies and how each analysis ended. It is the state a re-run compares with, so only changes become facts.
/// </summary>
/// <param name="Id">The prologue.</param>
/// <param name="Capabilities">The capabilities.</param>
/// <param name="DecisionPoints">The decision points.</param>
/// <param name="Dependencies">The dependencies.</param>
/// <param name="Runs">How the last analysis of each system or folder ended.</param>
[ReadModel]
public partial record CodeFindings(
    PrologueId Id,
    IEnumerable<MappedCapability> Capabilities,
    IEnumerable<MappedDecisionPoint> DecisionPoints,
    IEnumerable<MappedDependency> Dependencies,
    IEnumerable<AnalysisRun> Runs)
{
    /// <summary>
    /// Gets the findings of a prologue nothing was found for.
    /// </summary>
    /// <param name="id">The prologue.</param>
    /// <returns>The empty findings.</returns>
    public static CodeFindings Empty(PrologueId id) => new(id, [], [], [], []);

    /// <summary>
    /// Gets the run of a system or folder.
    /// </summary>
    /// <param name="subject">The system or folder.</param>
    /// <returns>The run, or null when it was never analyzed.</returns>
    public AnalysisRun? RunOf(AnalysisSubjectId subject) => Runs.FirstOrDefault(run => run.Subject == subject);
}
