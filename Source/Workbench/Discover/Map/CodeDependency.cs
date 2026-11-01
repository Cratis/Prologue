// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Linq;
using Cratis.Chronicle.ReadModels;
using Cratis.Prologue.Workbench.Discover.Findings;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Discover.Map;

/// <summary>
/// What talks to what: a system or folder and something it depends on.
/// </summary>
/// <param name="Subject">The system or folder that depends on the target.</param>
/// <param name="From">What it is called.</param>
/// <param name="Target">What it depends on.</param>
/// <param name="Kind">How it depends on it.</param>
/// <param name="Detail">How it is used.</param>
[ReadModel]
public record CodeDependency(AnalysisSubjectId Subject, SubjectName From, DependencyTarget Target, DependencyKind Kind, DependencyDetail Detail)
{
    /// <summary>
    /// Observes the dependencies of a prologue.
    /// </summary>
    /// <param name="prologue">The prologue.</param>
    /// <param name="readModels">Observes the stored findings and the prologue.</param>
    /// <returns>The dependencies.</returns>
    public static ISubject<IEnumerable<CodeDependency>> ObserveDependencies(PrologueId prologue, IReadModels readModels) =>
        new DerivedSubject<IEnumerable<CodeDependency>>(
            ApplicationMap.Findings(prologue, readModels).CombineLatest(ApplicationMap.Definition(prologue, readModels), Build));

    /// <summary>
    /// Names the dependencies in the findings.
    /// </summary>
    /// <param name="findings">What the analyses found.</param>
    /// <param name="definition">The prologue, which names the systems.</param>
    /// <returns>The dependencies.</returns>
    public static IEnumerable<CodeDependency> Build(CodeFindings findings, PrologueDefinition? definition) =>
        [.. findings.Dependencies.Select(_ => new CodeDependency(_.Subject, SystemNames.Of(_.Subject, definition, findings), _.Target, _.Kind, _.Detail))];
}
