// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Common.given;
using Cratis.Prologue.Workbench.Events.given;
using Cratis.Prologue.Workbench.Events.Suggestions;

namespace Cratis.Prologue.Workbench.Events.Reviewing.for_AcceptSuggestion;

public class when_accepting : a_library_interpretation
{
    static readonly SuggestionKey _key = "Authors/AuthorCreated";
    Result<IEnumerable<EventSuggestionAccepted>, ValidationResult> _first;
    Result<IEnumerable<EventSuggestionAccepted>, ValidationResult> _again;
    Result<IEnumerable<EventSuggestionAccepted>, ValidationResult> _rejected;
    Result<IEnumerable<EventSuggestionAccepted>, ValidationResult> _restored;
    Result<IEnumerable<EventSuggestionAccepted>, ValidationResult> _unknown;
    Result<IEnumerable<EventSuggestionAccepted>, ValidationResult> _nothingInterpreted;
    Result<IEnumerable<EventSuggestionAccepted>, ValidationResult> _withdrawn;

    void Because()
    {
        _first = new AcceptSuggestion(_prologue, _key).Handle(_review);
        var accepted = EventReview.Reduce(new EventSuggestionAccepted(_key), _review, _context);
        _again = new AcceptSuggestion(_prologue, _key).Handle(accepted);
        var rejectedReview = EventReview.Reduce(new EventSuggestionRejected(_key), _review, _context);
        _rejected = new AcceptSuggestion(_prologue, _key).Handle(rejectedReview);
        _restored = new AcceptSuggestion(_prologue, _key).Handle(EventReview.Reduce(new EventSuggestionRestored(_key), rejectedReview, _context));
        _unknown = new AcceptSuggestion(_prologue, "Nowhere/NothingHappened").Handle(_review);
        _nothingInterpreted = new AcceptSuggestion(_prologue, _key).Handle(null);
        _withdrawn = new AcceptSuggestion(_prologue, _key).Handle(EventReview.Reduce(new EventSuggestionsInterpreted([]), _review, _context));
    }

    static EventSuggestionAccepted[] Events(Result<IEnumerable<EventSuggestionAccepted>, ValidationResult> result) => result.TryGetResult(out var events) ? [.. events] : [];

    [Fact] void should_record_the_acceptance() => Events(_first).Single().Key.ShouldEqual(_key);
    [Fact] void should_record_nothing_when_it_is_accepted_already() => Events(_again).ShouldBeEmpty();
    [Fact] void should_not_refuse_accepting_it_again() => _again.TryGetError(out _).ShouldBeFalse();
    [Fact] void should_refuse_a_rejected_suggestion() => _rejected.TryGetError(out _).ShouldBeTrue();
    [Fact] void should_tell_to_restore_it_first() => _rejected.Refusal()!.Message.ShouldContain("Restore");
    [Fact] void should_accept_it_once_it_is_restored() => Events(_restored).Single().Key.ShouldEqual(_key);
    [Fact] void should_refuse_a_suggestion_that_does_not_exist() => _unknown.TryGetError(out _).ShouldBeTrue();
    [Fact] void should_refuse_when_nothing_was_interpreted() => _nothingInterpreted.TryGetError(out _).ShouldBeTrue();
    [Fact] void should_refuse_a_suggestion_the_evidence_no_longer_supports() => _withdrawn.TryGetError(out _).ShouldBeTrue();
}
#endif
