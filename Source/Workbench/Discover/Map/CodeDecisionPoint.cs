// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Linq;
using Cratis.Chronicle.ReadModels;
using Cratis.Prologue.Workbench.Discover.Findings;
using Cratis.Prologue.Workbench.Discover.Runtime;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Discover.Map;

/// <summary>
/// A decision point found in the code, with how often it happened at runtime. One that was never exercised is flagged as code only.
/// </summary>
/// <param name="Subject">The system or folder it was found in.</param>
/// <param name="System">What the system or folder is called.</param>
/// <param name="Rule">The rule in a sentence.</param>
/// <param name="File">The file it is in.</param>
/// <param name="Line">The line it is on.</param>
/// <param name="Shape">The command shape it belongs to.</param>
/// <param name="Kind">What it does when it decides.</param>
/// <param name="Happened">How often it happened at runtime.</param>
/// <param name="Status">The HTTP status it happened with most; zero when it has none.</param>
/// <param name="Runtime">Whether the runtime evidence matches it.</param>
[ReadModel]
public record CodeDecisionPoint(
    AnalysisSubjectId Subject,
    SubjectName System,
    RuleText Rule,
    SourcePath File,
    LineNumber Line,
    RouteShape Shape,
    DecisionKind Kind,
    int Happened,
    int Status,
    DecisionRuntime Runtime)
{
    /// <summary>
    /// Observes the decision points of a prologue, matched to what the running system did.
    /// </summary>
    /// <param name="prologue">The prologue.</param>
    /// <param name="readModels">Observes the stored findings and the prologue.</param>
    /// <param name="runtime">Supplies what was captured.</param>
    /// <returns>The decision points, ordered by file and line.</returns>
    public static ISubject<IEnumerable<CodeDecisionPoint>> ObserveDecisionPoints(PrologueId prologue, IReadModels readModels, IRuntimeShapes runtime) =>
        new DerivedSubject<IEnumerable<CodeDecisionPoint>>(
            ApplicationMap.Findings(prologue, readModels).CombineLatest(ApplicationMap.Definition(prologue, readModels), runtime.Observe(prologue), DecisionPointViews.Build));
}
