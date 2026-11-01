// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Captures;

namespace Cratis.Prologue.Workbench.Explore.Journeys.for_Journeys;

public class when_a_thing_that_was_let_go_comes_back : Specification
{
    JourneySequence[] _sequences;
    EntityTrail[] _trails;
    int _evicted;

    static RecordedCommand Step(int loan, string step) =>
        new("POST", $"/api/loans/{loan}/{step}", 200, TraceId.None, CommandShape.From("POST", $"/api/loans/{loan}/{step}"), 0);

    void Because()
    {
        IEnumerable<EntityTrail> trails = [];
        IEnumerable<EvictedEntity> evicted = [];
        IEnumerable<JourneySequence> sequences = [];
        for (var loan = 0; loan < JourneyInference.TrailsKept + 10; loan++)
        {
            (trails, evicted, sequences) = JourneyInference.Link(trails, evicted, sequences, Step(loan, "lend"));
        }

        // Loan 0 was let go; it is lent again and returned, but it is the same thing and was counted once.
        (trails, evicted, sequences) = JourneyInference.Link(trails, evicted, sequences, Step(0, "lend"));
        (trails, evicted, sequences) = JourneyInference.Link(trails, evicted, sequences, Step(0, "return"));

        _trails = [.. trails];
        _evicted = evicted.Count();
        _sequences = [.. JourneyInference.Describe(sequences)];
    }

    [Fact] void should_remember_the_things_it_let_go() => _evicted.ShouldEqual(10);
    [Fact] void should_not_count_it_as_a_new_thing() => _sequences.Sum(sequence => sequence.Entities).ShouldEqual(JourneyInference.TrailsKept + 10);
    [Fact] void should_not_start_following_it_again() => _trails.Any(trail => trail.Id.Value == "0").ShouldBeFalse();
}
#endif
