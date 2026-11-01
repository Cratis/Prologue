// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Events.given;
using Cratis.Prologue.Workbench.Events.Suggestions;

namespace Cratis.Prologue.Workbench.Events.Reviewing.for_RenameSuggestion;

public class when_renaming : a_library_interpretation
{
    static readonly SuggestionKey _key = "Authors/AuthorCreated";
    Result<IEnumerable<EventSuggestionRenamed>, ValidationResult> _renamed;
    Result<IEnumerable<EventSuggestionRenamed>, ValidationResult> _padded;
    Result<IEnumerable<EventSuggestionRenamed>, ValidationResult> _sameName;
    Result<IEnumerable<EventSuggestionRenamed>, ValidationResult> _notPascalCase;
    Result<IEnumerable<EventSuggestionRenamed>, ValidationResult> _withSpaces;
    Result<IEnumerable<EventSuggestionRenamed>, ValidationResult> _blank;
    Result<IEnumerable<EventSuggestionRenamed>, ValidationResult> _taken;
    Result<IEnumerable<EventSuggestionRenamed>, ValidationResult> _takenByARejectedOne;
    Result<IEnumerable<EventSuggestionRenamed>, ValidationResult> _notPastTense;
    Result<IEnumerable<EventSuggestionRenamed>, ValidationResult> _unknown;
    EventReview _afterwards;

    void Because()
    {
        _renamed = new RenameSuggestion(_prologue, _key, "AuthorRegistered").Handle(_review);
        _padded = new RenameSuggestion(_prologue, _key, " AuthorRegistered ").Handle(_review);
        _sameName = new RenameSuggestion(_prologue, _key, "AuthorCreated").Handle(_review);
        _notPascalCase = new RenameSuggestion(_prologue, _key, "authorRegistered").Handle(_review);
        _withSpaces = new RenameSuggestion(_prologue, _key, "Author Registered").Handle(_review);
        _blank = new RenameSuggestion(_prologue, _key, "  ").Handle(_review);
        _taken = new RenameSuggestion(_prologue, _key, "AuthorDeleted").Handle(_review);
        _takenByARejectedOne = new RenameSuggestion(_prologue, _key, "AuthorDeleted").Handle(EventReview.Reduce(new EventSuggestionRejected("Authors/AuthorDeleted"), _review, _context));
        _notPastTense = new RenameSuggestion(_prologue, _key, "AuthorRegistration").Handle(_review);
        _unknown = new RenameSuggestion(_prologue, "Nowhere/NothingHappened", "SomethingHappened").Handle(_review);
        _afterwards = EventReview.Reduce(_renamed.TryGetResult(out var events) ? events.Single() : null!, _review, _context);
    }

    static EventSuggestionRenamed[] Events(Result<IEnumerable<EventSuggestionRenamed>, ValidationResult> result) => result.TryGetResult(out var events) ? [.. events] : [];

    [Fact] void should_record_the_name() => Events(_renamed).Single().Name.Value.ShouldEqual("AuthorRegistered");
    [Fact] void should_show_the_name_in_the_review() => EventReview.Find(_afterwards, _key)!.Name.Value.ShouldEqual("AuthorRegistered");
    [Fact] void should_remember_that_a_person_chose_it() => EventReview.Find(_afterwards, _key)!.Renamed.ShouldBeTrue();
    [Fact] void should_keep_the_mechanical_name() => EventReview.Find(_afterwards, _key)!.Suggestion.Mechanical.Value.ShouldEqual("AuthorCreated");
    [Fact] void should_trim_the_name() => Events(_padded).Single().Name.Value.ShouldEqual("AuthorRegistered");
    [Fact] void should_record_nothing_when_the_name_is_the_one_it_has() => Events(_sameName).ShouldBeEmpty();
    [Fact] void should_refuse_a_name_that_is_not_pascal_case() => _notPascalCase.TryGetError(out _).ShouldBeTrue();
    [Fact] void should_refuse_a_name_with_spaces() => _withSpaces.TryGetError(out _).ShouldBeTrue();
    [Fact] void should_refuse_a_blank_name() => _blank.TryGetError(out _).ShouldBeTrue();
    [Fact] void should_refuse_a_name_another_event_has() => _taken.TryGetError(out _).ShouldBeTrue();
    [Fact] void should_allow_a_name_only_a_rejected_event_has() => _takenByARejectedOne.TryGetError(out _).ShouldBeFalse();
    [Fact] void should_only_advise_about_a_name_that_is_not_past_tense() => Events(_notPastTense).Single().Name.Value.ShouldEqual("AuthorRegistration");
    [Fact] void should_refuse_a_suggestion_that_does_not_exist() => _unknown.TryGetError(out _).ShouldBeTrue();
}
#endif
