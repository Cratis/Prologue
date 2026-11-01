// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Events.given;
using Cratis.Prologue.Workbench.Events.Reviewing;

namespace Cratis.Prologue.Workbench.Events.Suggestions.for_EventReview;

public class when_a_new_interpretation_arrives_after_a_review : a_library_interpretation
{
    static readonly SuggestionKey _author = "Authors/AuthorCreated";
    static readonly SuggestionKey _delete = "Authors/AuthorDeleted";
    static readonly SuggestionKey _loan = "Loans/LoanCreated";
    EventReview _afterwards;

    void Because()
    {
        var decided = EventReview.Reduce(new EventSuggestionAccepted(_author), _review, _context);
        decided = EventReview.Reduce(new EventSuggestionRenamed(_author, "AuthorRegistered"), decided, _context);
        decided = EventReview.Reduce(new EventSuggestionRejected(_delete), decided, _context);
        var refined = _suggestions.Where(suggestion => suggestion.Key != _loan).Select(suggestion => suggestion with { Refined = $"{suggestion.Refined.Value}Again" });
        _afterwards = EventReview.Reduce(new EventSuggestionsInterpreted(refined), decided, _context);
    }

    ReviewedSuggestion Reviewed(SuggestionKey key) => EventReview.Find(_afterwards, key)!;

    [Fact] void should_keep_what_was_accepted() => Reviewed(_author).Status.ShouldEqual(SuggestionStatus.Accepted);
    [Fact] void should_keep_the_name_a_person_chose() => Reviewed(_author).Name.Value.ShouldEqual("AuthorRegistered");
    [Fact] void should_keep_what_was_rejected() => Reviewed(_delete).Status.ShouldEqual(SuggestionStatus.Rejected);
    [Fact] void should_take_the_new_refined_name_for_what_no_person_renamed() => Reviewed(_delete).Name.Value.ShouldEqual("AuthorDeletedAgain");
    [Fact] void should_take_the_new_evidence() => Reviewed(_author).Suggestion.Refined.Value.ShouldEqual("AuthorCreatedAgain");
    [Fact] void should_withdraw_what_is_no_longer_found() => Reviewed(_loan).Withdrawn.ShouldBeTrue();
    [Fact] void should_not_lose_what_is_no_longer_found() => EventReview.Find(_afterwards, _loan).ShouldNotBeNull();
    [Fact] void should_not_list_a_suggestion_twice() => _afterwards.Suggestions.Select(suggestion => suggestion.Suggestion.Key).Distinct().Count().ShouldEqual(_afterwards.Suggestions.Count());
}
#endif
