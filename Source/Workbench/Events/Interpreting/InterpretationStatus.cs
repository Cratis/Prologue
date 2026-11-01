// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Linq;
using Cratis.Prologue.Workbench.Discover.Map;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Events.Interpreting;

/// <summary>
/// How the interpretation of a prologue is going, for the Events page.
/// </summary>
/// <param name="Id">The prologue.</param>
/// <param name="State">Where the interpretation is.</param>
/// <param name="Message">What is happening, or what happened, in words.</param>
/// <param name="Refined">Whether the language model refined the last result.</param>
/// <param name="Suggestions">How many events the last finished interpretation suggested.</param>
[ReadModel]
public record InterpretationStatus(PrologueId Id, InterpretationState State, string Message, bool Refined, int Suggestions)
{
    /// <summary>
    /// Observes how the interpretation of a prologue is going.
    /// </summary>
    /// <param name="id">The prologue.</param>
    /// <param name="progress">Holds the progress.</param>
    /// <returns>The status, pushed whenever it changes.</returns>
    public static ISubject<InterpretationStatus> ObserveInterpretationStatus(PrologueId id, IInterpretationProgress progress) =>
        new DerivedSubject<InterpretationStatus>(progress.Observe(id).Select(current => new InterpretationStatus(id, current.State, current.Message, current.Refined, current.Suggestions)));
}
