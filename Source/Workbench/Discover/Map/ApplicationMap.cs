// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Linq;
using Cratis.Chronicle.ReadModels;
using Cratis.Prologue.Workbench.Discover.Findings;
using Cratis.Prologue.Workbench.Discover.Runtime;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Discover.Map;

/// <summary>
/// The application map of a prologue: its systems, their modules and capabilities, each marked as seen at runtime or found
/// only in code, with the numbers the Discover tiles show.
/// </summary>
/// <param name="Id">The prologue.</param>
/// <param name="Systems">The systems and folders with their modules.</param>
/// <param name="Capabilities">The number of capabilities mapped.</param>
/// <param name="MatchedToRuntime">How many were also seen at runtime.</param>
/// <param name="CodeOnly">How many were found only in code.</param>
/// <param name="DecisionPoints">The number of decision points found.</param>
/// <param name="FilesRead">The number of files the last analyses read.</param>
[ReadModel]
public record ApplicationMap(PrologueId Id, IEnumerable<MapSystem> Systems, int Capabilities, int MatchedToRuntime, int CodeOnly, int DecisionPoints, int FilesRead)
{
    /// <summary>
    /// Observes the application map of a prologue. It changes when an analysis records a finding and when the captures change
    /// what was seen at runtime.
    /// </summary>
    /// <param name="prologue">The prologue.</param>
    /// <param name="readModels">Observes the stored findings and the prologue.</param>
    /// <param name="runtime">Supplies what was captured.</param>
    /// <returns>The map.</returns>
    public static ISubject<ApplicationMap> ObserveApplicationMap(PrologueId prologue, IReadModels readModels, IRuntimeShapes runtime) =>
        new DerivedSubject<ApplicationMap>(
            Findings(prologue, readModels).CombineLatest(Definition(prologue, readModels), runtime.Observe(prologue), ApplicationMapBuilder.Build));

    internal static IObservable<CodeFindings> Findings(PrologueId prologue, IReadModels readModels) =>
        readModels.Observe<CodeFindings>(findings => findings.Id == prologue).Select(found => found.FirstOrDefault() ?? CodeFindings.Empty(prologue));

    internal static IObservable<PrologueDefinition?> Definition(PrologueId prologue, IReadModels readModels) =>
        readModels.Observe<PrologueDefinition>(definition => definition.Id == prologue).Select(found => found.FirstOrDefault());
}
