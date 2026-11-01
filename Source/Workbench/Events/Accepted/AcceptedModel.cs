// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Linq;
using Cratis.Chronicle.ReadModels;
using Cratis.Prologue.Workbench.Discover.Map;
using Cratis.Prologue.Workbench.Events.Suggestions;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Events.Accepted;

/// <summary>
/// The event model of a prologue as the review has shaped it so far: modules, features and slices with their commands, the events people accepted
/// under the names they chose, the read models and projections the interpretation found, the rules and constraints the evidence and the
/// consistency-boundary choices add, and the events still open, marked as such.
/// </summary>
/// <remarks>
/// <para>
/// This is the contract the Model step builds on. <see cref="AcceptedModelMapper.ToExtractionResult"/> turns it into the
/// <c language="csharp">ExtractionResult</c> the <c language="csharp">ScreenplayGenerator</c> takes; <see cref="AcceptedSlice.Shapes"/> give the command shapes for
/// <c language="csharp">AddRebuildSlice(prologue, name, shapes)</c>; and once the model is accepted, <c language="csharp">AcceptModelForRebuild</c> hands it to drift detection.
/// </para>
/// <para>
/// A rejected event is not in the model. An event nobody has decided on is in it with <see cref="AcceptedEvent.IsOpen"/>, because a model
/// that waits for every decision cannot be looked at; the Model step shows it as a draft. A slice whose events were all rejected is left out.
/// </para>
/// </remarks>
/// <param name="Id">The prologue.</param>
/// <param name="SystemName">The name of the system as the language model derived it; empty when it did not.</param>
/// <param name="Accepted">How many events are accepted.</param>
/// <param name="Open">How many events are still open.</param>
/// <param name="Rejected">How many suggested events are rejected.</param>
/// <param name="OpenBoundaries">How many consistency boundaries nobody has decided yet.</param>
/// <param name="Modules">The modules.</param>
/// <param name="Rules">Every rule and constraint of the model, with the command shape it is about.</param>
[ReadModel]
public record AcceptedModel(
    PrologueId Id,
    string SystemName,
    int Accepted,
    int Open,
    int Rejected,
    int OpenBoundaries,
    IEnumerable<AcceptedModule> Modules,
    IEnumerable<AcceptedRule> Rules)
{
    /// <summary>
    /// Observes the accepted model of a prologue.
    /// </summary>
    /// <param name="id">The prologue.</param>
    /// <param name="readModels">Observes the review.</param>
    /// <returns>The model, pushed whenever the review changes.</returns>
    public static ISubject<AcceptedModel> ObserveAcceptedModel(PrologueId id, IReadModels readModels) =>
        new DerivedSubject<AcceptedModel>(
            readModels.Observe<EventReview>(review => review.Id == id).StartWith([]).Select(found => AcceptedModels.From(id, found.FirstOrDefault())));
}
