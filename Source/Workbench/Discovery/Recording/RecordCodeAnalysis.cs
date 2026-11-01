// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Arc.Chronicle.ReadModels;
using Cratis.Prologue.Workbench.Discovery.Findings;
using Cratis.Prologue.Workbench.Prologues;
using Cratis.Prologue.Workbench.Repositories.Cloning;

namespace Cratis.Prologue.Workbench.Discovery.Recording;

/// <summary>
/// Records what an analysis found. It is run by the analysis worker when a job ends, never by a person: it compares the
/// outcome with what is recorded and appends only what was added, changed or is gone.
/// </summary>
/// <param name="Prologue">The prologue.</param>
/// <param name="Subject">The system or folder.</param>
/// <param name="Name">What it is called.</param>
/// <param name="Source">The commit or folder that was read.</param>
/// <param name="Files">The number of files read.</param>
/// <param name="Capabilities">The capabilities found.</param>
/// <param name="DecisionPoints">The decision points found.</param>
/// <param name="Dependencies">The dependencies found.</param>
/// <param name="Failure">Why the analysis could not finish, when it could not.</param>
[Command]
public record RecordCodeAnalysis(
    PrologueId Prologue,
    AnalysisSubjectId Subject,
    SubjectName Name,
    AnalysisSource Source,
    FileCount Files,
    IEnumerable<FoundCapability> Capabilities,
    IEnumerable<FoundDecisionPoint> DecisionPoints,
    IEnumerable<FoundDependency> Dependencies,
    FailureReason? Failure) : ICanProvideEventSourceId
{
    /// <inheritdoc/>
    public EventSourceId GetEventSourceId() => Prologue;

    /// <summary>
    /// Handles the command by deciding which of the findings are facts worth recording.
    /// </summary>
    /// <param name="current">What is recorded already for the prologue.</param>
    /// <returns>The events to append; none when the analysis found nothing new.</returns>
    public IEnumerable<object> Handle([Unprotected("Analyses run one at a time; comparing with a read model that trails the log costs at most one repeated fact, never a lost change")] CodeFindings? current) =>
        CodeAnalysisDecision.Decide(new AnalysisOutcome(Subject, Name, Source, Files, new AnalysisFindings(Capabilities, DecisionPoints, Dependencies), Failure), current);
}
