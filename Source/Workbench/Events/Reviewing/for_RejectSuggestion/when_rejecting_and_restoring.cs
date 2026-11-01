// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Events.given;
using Cratis.Prologue.Workbench.Events.Suggestions;

namespace Cratis.Prologue.Workbench.Events.Reviewing.for_RejectSuggestion;

public class when_rejecting_and_restoring : a_library_interpretation
{
    static readonly SuggestionKey _key = "Authors/AuthorCreated";
    Result<IEnumerable<EventSuggestionRejected>, ValidationResult> _reject;
    Result<IEnumerable<EventSuggestionRejected>, ValidationResult> _rejectAgain;
    Result<IEnumerable<EventSuggestionRejected>, ValidationResult> _rejectAccepted;
    Result<IEnumerable<EventSuggestionRejected>, ValidationResult> _rejectUnknown;
    Result<IEnumerable<EventSuggestionRestored>, ValidationResult> _restore;
    Result<IEnumerable<EventSuggestionRestored>, ValidationResult> _restoreWhatIsNotRejected;
    Result<IEnumerable<EventSuggestionRestored>, ValidationResult> _restoreUnknown;
    EventReview _rejected;
    EventReview _restored;

    void Because()
    {
        _reject = new RejectSuggestion(_prologue, _key).Handle(_review);
        _rejected = EventReview.Reduce(new EventSuggestionRejected(_key), _review, _context);
        _rejectAgain = new RejectSuggestion(_prologue, _key).Handle(_rejected);
        _rejectAccepted = new RejectSuggestion(_prologue, _key).Handle(EventReview.Reduce(new EventSuggestionAccepted(_key), _review, _context));
        _rejectUnknown = new RejectSuggestion(_prologue, "Nowhere/NothingHappened").Handle(_review);
        _restore = new RestoreSuggestion(_prologue, _key).Handle(_rejected);
        _restored = EventReview.Reduce(new EventSuggestionRestored(_key), _rejected, _context);
        _restoreWhatIsNotRejected = new RestoreSuggestion(_prologue, _key).Handle(_review);
        _restoreUnknown = new RestoreSuggestion(_prologue, "Nowhere/NothingHappened").Handle(_review);
    }

    static T[] Events<T>(Result<IEnumerable<T>, ValidationResult> result) => result.TryGetResult(out var events) ? [.. events] : [];

    [Fact] void should_record_the_rejection() => Events(_reject).Single().Key.ShouldEqual(_key);
    [Fact] void should_mark_the_suggestion_rejected() => EventReview.Find(_rejected, _key)!.Status.ShouldEqual(SuggestionStatus.Rejected);
    [Fact] void should_keep_a_rejected_suggestion_in_the_review() => _rejected.Suggestions.Count().ShouldEqual(_review.Suggestions.Count());
    [Fact] void should_record_nothing_when_it_is_rejected_already() => Events(_rejectAgain).ShouldBeEmpty();
    [Fact] void should_reject_what_was_accepted() => Events(_rejectAccepted).Single().Key.ShouldEqual(_key);
    [Fact] void should_refuse_rejecting_a_suggestion_that_does_not_exist() => _rejectUnknown.TryGetError(out _).ShouldBeTrue();
    [Fact] void should_record_the_restoration() => Events(_restore).Single().Key.ShouldEqual(_key);
    [Fact] void should_bring_the_suggestion_back_to_review() => EventReview.Find(_restored, _key)!.Status.ShouldEqual(SuggestionStatus.Suggested);
    [Fact] void should_record_nothing_when_restoring_what_is_not_rejected() => Events(_restoreWhatIsNotRejected).ShouldBeEmpty();
    [Fact] void should_refuse_restoring_a_suggestion_that_does_not_exist() => _restoreUnknown.TryGetError(out _).ShouldBeTrue();
}
#endif
