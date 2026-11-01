// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Captures;

namespace Cratis.Prologue.Workbench.Explore.Journeys.for_Journeys;

public class when_more_things_arrive_than_are_followed : Specification
{
    EntityTrail[] _trails;
    JourneySequence[] _sequences;

    void Because()
    {
        IEnumerable<EntityTrail> trails = [];
        IEnumerable<EvictedEntity> evicted = [];
        IEnumerable<JourneySequence> sequences = [];
        for (var loan = 0; loan < JourneyInference.TrailsKept + 5; loan++)
        {
            var command = new RecordedCommand("POST", $"/api/loans/{loan}/lend", 200, TraceId.None, CommandShape.From("POST", "/api/loans/1/lend"), 0);
            (trails, evicted, sequences) = JourneyInference.Link(trails, evicted, sequences, command);
        }

        _trails = [.. trails];
        _sequences = [.. JourneyInference.Describe(sequences)];
    }

    [Fact] void should_follow_no_more_than_the_limit() => _trails.Length.ShouldEqual(JourneyInference.TrailsKept);
    [Fact] void should_let_go_of_the_things_untouched_the_longest() => _trails.Any(trail => trail.Id.Value == "0").ShouldBeFalse();
    [Fact] void should_keep_counting_the_things_let_go() => _sequences.Single().Entities.ShouldEqual(JourneyInference.TrailsKept + 5);
}
#endif
