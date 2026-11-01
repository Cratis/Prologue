// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Captures.given;
using Cratis.Prologue.Workbench.Captures.Recording;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Explore.Journeys.for_Journeys;

public class when_captures_of_things_arrive : Specification
{
    Journeys _journeys;

    // Loans 1..4 are lent and returned, 5..7 are lent and never returned, 8 is lent, returned and returned again.
    void Because()
    {
        var context = EventContext.EmptyWithEventSourceId(PrologueId.New());
        var events = new List<CaptureRecorded>();
        for (var loan = 1; loan <= 8; loan++)
        {
            events.Add(RecordedCaptures.Command("POST", $"/api/loans/{loan}/lend", 201));
            events.Add(RecordedCaptures.Command("GET", $"/api/loans/{loan}", 200));
        }

        foreach (var loan in new[] { 1, 2, 3, 4, 8, 8 })
        {
            events.Add(RecordedCaptures.Command("POST", $"/api/loans/{loan}/return", 200));
        }

        events.Add(RecordedCaptures.Command("POST", "/api/members", 201));
        events.Add(RecordedCaptures.WithoutCommand());

        foreach (var @event in events)
        {
            _journeys = Journeys.Reduce(@event, _journeys, context);
        }
    }

    JourneySequence Sequence(params string[] steps) =>
        _journeys.Sequences.Single(sequence => sequence.Steps.Select(step => step.Value).SequenceEqual(steps));

    [Fact] void should_follow_one_trail_per_thing() => _journeys.Trails.Count().ShouldEqual(8);
    [Fact] void should_count_the_things_that_went_through_each_sequence() => Sequence("POST /api/loans/{id}/lend", "POST /api/loans/{id}/return").Entities.ShouldEqual(5);
    [Fact] void should_count_things_that_stopped_at_the_first_step() => Sequence("POST /api/loans/{id}/lend").Entities.ShouldEqual(3);
    [Fact] void should_give_the_share_among_things_of_the_kind() => Sequence("POST /api/loans/{id}/lend", "POST /api/loans/{id}/return").Share.ShouldEqual(63);
    [Fact] void should_note_where_the_unfinished_ones_would_have_gone() => Sequence("POST /api/loans/{id}/lend").ContinuedBy.Value.ShouldEqual("POST /api/loans/{id}/return");
    [Fact] void should_not_call_the_longest_sequence_unfinished() => Sequence("POST /api/loans/{id}/lend", "POST /api/loans/{id}/return").ContinuedBy.IsNone.ShouldBeTrue();
    [Fact] void should_count_a_repeated_step_once() => _journeys.Sequences.Any(sequence => sequence.Steps.Count() > 2).ShouldBeFalse();
    [Fact] void should_leave_out_reads() => _journeys.Sequences.Any(sequence => sequence.Steps.Any(step => step.Value.StartsWith("GET", StringComparison.Ordinal))).ShouldBeFalse();
    [Fact] void should_leave_out_commands_without_an_identity_in_the_path() => _journeys.Sequences.Any(sequence => sequence.Steps.Any(step => step.Value == "POST /api/members")).ShouldBeFalse();
    [Fact] void should_list_the_most_common_sequence_first() => _journeys.Sequences.First().Entities.ShouldEqual(5);
}
#endif
