// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Linq;
using Cratis.Chronicle.ReadModels;
using Cratis.Prologue.Screenplay;
using Cratis.Prologue.Workbench.Discover.Map;
using Cratis.Prologue.Workbench.Events.Accepted;
using Cratis.Prologue.Workbench.Events.Suggestions;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Model.Export;

/// <summary>
/// The Screenplay of the provisional event model of a prologue, regenerated whenever a suggestion is accepted or rejected.
/// </summary>
/// <param name="Id">The <see cref="PrologueId"/>.</param>
/// <param name="Play">The <c language="csharp">.play</c> text; empty when the model holds nothing yet.</param>
/// <param name="Drafts">How many slices still hold open suggestions and are marked as drafts in the text.</param>
[ReadModel]
public record ModelScreenplay(PrologueId Id, string Play, int Drafts)
{
    /// <summary>
    /// Observes the Screenplay of a prologue's model.
    /// </summary>
    /// <param name="id">The <see cref="PrologueId"/>.</param>
    /// <param name="readModels">Reads the review the model is built from.</param>
    /// <param name="generator">Generates the Screenplay.</param>
    /// <returns>A subject of the <see cref="ModelScreenplay"/>.</returns>
    public static ISubject<ModelScreenplay> ObserveModelScreenplay(PrologueId id, IReadModels readModels, IScreenplayGenerator generator) =>
        new DerivedSubject<ModelScreenplay>(
            readModels.Observe<EventReview>(review => review.Id == id).StartWith([]).Select(found => ModelScreenplays.From(AcceptedModels.From(id, found.FirstOrDefault()), generator)));
}
