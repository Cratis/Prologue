// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Captures;
using Cratis.Prologue.Workbench.Events.given;
using Cratis.Prologue.Workbench.Events.Reviewing;
using Cratis.Prologue.Workbench.Events.Uncaptured;

namespace Cratis.Prologue.Workbench.Events.Suggestions.for_ReviewMerge;

public class when_code_only_suggestions_change : an_uncaptured_library
{
    static readonly SuggestionKey _renewal = "Lending/LoanRenewed";
    static readonly SuggestionKey _reservation = "Reservations/ReservationCreated";

    EventReview _decided;
    EventReview _stillOnlyInCode;
    EventReview _gone;
    EventReview _backedByCaptures;
    EventReview _backedUnderTheSameKey;
    EventReview _backedAfterARejection;
    EventReview _undecidedBackedByCaptures;

    SuggestedEvent Captured(string key, string shape, string module = "Lending", string name = "LoanRenewedAgain") =>
        _suggestions[0] with { Key = key, Mechanical = name, Refined = name, Module = module, Evidence = new SuggestionEvidence([new ShapeEvidence(new CommandShape(shape), 3)], [], [], []), Origin = SuggestionOrigin.Captured };

    ReviewedSuggestion Reviewed(EventReview review, SuggestionKey key) => EventReview.Find(review, key)!;

    void Because()
    {
        var (all, _, _) = UncapturedEvents.Extend(_suggestions, _model, _uncaptured, []);
        var interpreted = EventReview.Reduce(new EventSuggestionsInterpreted(all), null, _context);
        _decided = EventReview.Reduce(new EventSuggestionAccepted(_renewal), interpreted, _context);
        _decided = EventReview.Reduce(new EventSuggestionRenamed(_renewal, "LoanExtended"), _decided, _context);
        _decided = EventReview.Reduce(new EventSuggestionRejected(_reservation), _decided, _context);

        _stillOnlyInCode = EventReview.Reduce(new EventSuggestionsInterpreted(all), _decided, _context);
        _gone = EventReview.Reduce(new EventSuggestionsInterpreted(all.Where(suggestion => suggestion.Key != _renewal)), _decided, _context);

        var captured = _suggestions.Append(Captured("Lending/LoanExtended", "post /api/loans/{id}/renew"));
        _backedByCaptures = EventReview.Reduce(new EventSuggestionsInterpreted(captured), _decided, _context);

        var sameKey = _suggestions.Append(Captured("Lending/LoanRenewed", "POST /api/loans/{id}/renew", name: "LoanRenewed"));
        _backedUnderTheSameKey = EventReview.Reduce(new EventSuggestionsInterpreted(sameKey), _decided, _context);

        var elsewhere = _suggestions.Append(Captured("Reservations/ReservationBooked", "POST /api/reservations", "Reservations", "ReservationBooked"));
        _backedAfterARejection = EventReview.Reduce(new EventSuggestionsInterpreted(elsewhere), _decided, _context);

        var undecided = EventReview.Reduce(new EventSuggestionsInterpreted(all), null, _context);
        _undecidedBackedByCaptures = EventReview.Reduce(new EventSuggestionsInterpreted(elsewhere), undecided, _context);
    }

    [Fact] void should_keep_the_decisions_when_the_code_still_has_the_capability() => Reviewed(_stillOnlyInCode, _renewal).Status.ShouldEqual(SuggestionStatus.Accepted);
    [Fact] void should_keep_the_name_a_person_chose_for_it() => Reviewed(_stillOnlyInCode, _renewal).Name.Value.ShouldEqual("LoanExtended");
    [Fact] void should_keep_a_rejection() => Reviewed(_stillOnlyInCode, _reservation).Status.ShouldEqual(SuggestionStatus.Rejected);
    [Fact] void should_withdraw_it_when_the_capability_disappears() => Reviewed(_gone, _renewal).Withdrawn.ShouldBeTrue();
    [Fact] void should_not_lose_it_when_the_capability_disappears() => Reviewed(_gone, _renewal).Status.ShouldEqual(SuggestionStatus.Accepted);
    [Fact] void should_let_the_captured_suggestion_take_over_when_the_command_is_captured() => Reviewed(_backedByCaptures, "Lending/LoanExtended").Suggestion.Origin.ShouldEqual(SuggestionOrigin.Captured);
    [Fact] void should_carry_the_acceptance_to_the_captured_suggestion() => Reviewed(_backedByCaptures, "Lending/LoanExtended").Status.ShouldEqual(SuggestionStatus.Accepted);
    [Fact] void should_carry_the_name_to_the_captured_suggestion() => Reviewed(_backedByCaptures, "Lending/LoanExtended").Name.Value.ShouldEqual("LoanExtended");
    [Fact] void should_drop_the_code_only_suggestion_it_replaces() => EventReview.Find(_backedByCaptures, _renewal).ShouldBeNull();
    [Fact] void should_not_drop_the_other_code_only_suggestions() => EventReview.Find(_backedByCaptures, _reservation).ShouldNotBeNull();
    [Fact] void should_keep_the_decision_when_the_captured_suggestion_has_the_same_key() => Reviewed(_backedUnderTheSameKey, _renewal).Status.ShouldEqual(SuggestionStatus.Accepted);
    [Fact] void should_take_the_captures_as_the_evidence_under_the_same_key() => Reviewed(_backedUnderTheSameKey, _renewal).Suggestion.Origin.ShouldEqual(SuggestionOrigin.Captured);
    [Fact] void should_carry_a_rejection_to_the_captured_suggestion() => Reviewed(_backedAfterARejection, "Reservations/ReservationBooked").Status.ShouldEqual(SuggestionStatus.Rejected);
    [Fact] void should_drop_an_undecided_code_only_suggestion_once_its_command_is_captured() => EventReview.Find(_undecidedBackedByCaptures, _reservation).ShouldBeNull();
    [Fact] void should_not_list_a_suggestion_twice() => _backedByCaptures.Suggestions.Select(suggestion => suggestion.Suggestion.Key).Distinct().Count().ShouldEqual(_backedByCaptures.Suggestions.Count());
}
#endif
