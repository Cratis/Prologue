// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Events.given;
using Cratis.Prologue.Workbench.Events.Reviewing;
using Cratis.Prologue.Workbench.Events.Suggestions;
using Cratis.Prologue.Workbench.Events.Uncaptured;

namespace Cratis.Prologue.Workbench.Events.Accepted.for_AcceptedModels;

public class when_code_only_events_are_in_the_model : an_uncaptured_library
{
    static readonly SuggestionKey _renewal = "Lending/LoanRenewed";
    AcceptedModel _pending;
    AcceptedModel _accepted;

    AcceptedEvent Renewal(AcceptedModel model) => model.Modules.SelectMany(module => module.Features).SelectMany(feature => feature.Slices).SelectMany(slice => slice.Events).Single(@event => @event.Key == _renewal);

    void Because()
    {
        var (all, model, _) = UncapturedEvents.Extend(_suggestions, _model, _uncaptured, []);
        var review = EventReview.Reduce(new EventSuggestionsInterpreted(all), null, _context);
        review = EventReview.Reduce(new InterpretedModelRecorded(model), review, _context);
        _pending = AcceptedModels.From(_prologue, review);
        _accepted = AcceptedModels.From(_prologue, EventReview.Reduce(new EventSuggestionAccepted(_renewal), review, _context));
    }

    [Fact] void should_show_the_event_in_a_slice_of_the_model() => Renewal(_pending).Name.Value.ShouldEqual("LoanRenewed");
    [Fact] void should_show_it_as_open_until_it_is_accepted() => Renewal(_pending).IsOpen.ShouldBeTrue();
    [Fact] void should_show_its_slice_as_open() => _pending.Modules.SelectMany(module => module.Features).SelectMany(feature => feature.Slices).Single(slice => slice.Events.Any(@event => @event.Key == _renewal)).IsOpen.ShouldBeTrue();
    [Fact] void should_show_it_as_decided_once_accepted() => Renewal(_accepted).IsOpen.ShouldBeFalse();
    [Fact] void should_count_it_among_the_open_events() => _pending.Open.ShouldBeGreaterThan(_accepted.Open);
}
#endif
