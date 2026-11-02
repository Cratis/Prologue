// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Discover.Findings;
using Cratis.Prologue.Workbench.Events.given;
using Cratis.Prologue.Workbench.Events.Suggestions;

namespace Cratis.Prologue.Workbench.Events.Uncaptured.for_CodeOnlySuggestions;

public class when_two_capabilities_imply_the_same_event : an_uncaptured_library
{
    IReadOnlyList<(SuggestedEvent Suggestion, MappedCapability Capability)> _found;

    protected override IEnumerable<MappedCapability> Capabilities() =>
        [Capability("Reservations", "Reserve a book", "POST /api/reservations"), Capability("Reservations", "Reserve a book again", "POST /api/reservations/bulk"), Capability("Reservations", "Reserve a book", "POST /api/v2/reservations")];

    void Because() => _found = CodeOnlySuggestions.From(_uncaptured, new HashSet<SuggestionKey>(), new HashSet<string>());

    [Fact] void should_suggest_one_event_per_key() => _found.Select(item => item.Suggestion.Key).Distinct().Count().ShouldEqual(_found.Count);
    [Fact] void should_keep_the_evidence_of_the_capability_that_came_second() => _found.Single(item => item.Suggestion.Key.Value == "Reservations/ReservationCreated").Suggestion.Evidence.Shapes.Select(shape => shape.Shape.Value).ShouldContain("POST /api/v2/reservations");
    [Fact] void should_say_which_capability_it_also_follows_from() => _found.Single(item => item.Suggestion.Key.Value == "Reservations/ReservationCreated").Suggestion.Notes.Any(note => note.Text.Contains("also follows")).ShouldBeTrue();
}
#endif
