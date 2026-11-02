// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Captures;
using Cratis.Prologue.Workbench.Events.given;
using Cratis.Prologue.Workbench.Events.Reviewing;
using Cratis.Prologue.Workbench.Events.Uncaptured;

namespace Cratis.Prologue.Workbench.Events.Suggestions.for_ReviewMerge;

public class when_a_withdrawn_code_only_suggestion_becomes_captured : an_uncaptured_library
{
    static readonly SuggestionKey _renewal = "Lending/LoanRenewed";
    static readonly SuggestionKey _reservation = "Reservations/ReservationCreated";

    EventReview _decidedThenWithdrawn;
    EventReview _captured;

    SuggestedEvent Captured(string key, string shape, string module, string name) =>
        _suggestions[0] with { Key = key, Mechanical = name, Refined = name, Module = module, Evidence = new SuggestionEvidence([new ShapeEvidence(new CommandShape(shape), 3)], [], [], []), Origin = SuggestionOrigin.Captured };

    ReviewedSuggestion Reviewed(SuggestionKey key) => EventReview.Find(_captured, key)!;

    void Because()
    {
        var (all, _, _) = UncapturedEvents.Extend(_suggestions, _model, _uncaptured, []);
        var interpreted = EventReview.Reduce(new EventSuggestionsInterpreted(all), null, _context);
        var decided = EventReview.Reduce(new EventSuggestionAccepted(_renewal), interpreted, _context);
        decided = EventReview.Reduce(new EventSuggestionRenamed(_renewal, "LoanExtended"), decided, _context);
        decided = EventReview.Reduce(new EventSuggestionRejected(_reservation), decided, _context);

        // The code no longer shows either capability, so both are withdrawn with their decisions.
        var withoutThem = all.Where(suggestion => suggestion.Key != _renewal && suggestion.Key != _reservation);
        _decidedThenWithdrawn = EventReview.Reduce(new EventSuggestionsInterpreted(withoutThem), decided, _context);

        // Later the commands are captured, under names of their own.
        var found = withoutThem
            .Append(Captured("Lending/LoanExtended", "POST /api/loans/{id}/renew", "Lending", "LoanExtended"))
            .Append(Captured("Reservations/ReservationBooked", "POST /api/reservations", "Reservations", "ReservationBooked"));
        _captured = EventReview.Reduce(new EventSuggestionsInterpreted(found), _decidedThenWithdrawn, _context);
    }

    [Fact] void should_have_withdrawn_it_first() => EventReview.Find(_decidedThenWithdrawn, _renewal)!.Withdrawn.ShouldBeTrue();
    [Fact] void should_carry_the_acceptance_to_the_captured_suggestion() => Reviewed("Lending/LoanExtended").Status.ShouldEqual(SuggestionStatus.Accepted);
    [Fact] void should_carry_the_name_a_person_chose() => Reviewed("Lending/LoanExtended").Name.Value.ShouldEqual("LoanExtended");
    [Fact] void should_carry_a_rejection_too() => Reviewed("Reservations/ReservationBooked").Status.ShouldEqual(SuggestionStatus.Rejected);
    [Fact] void should_retire_the_withdrawn_one() => EventReview.Find(_captured, _renewal).ShouldBeNull();
    [Fact] void should_not_show_the_captured_suggestion_as_withdrawn() => Reviewed("Lending/LoanExtended").Withdrawn.ShouldBeFalse();
}
#endif
