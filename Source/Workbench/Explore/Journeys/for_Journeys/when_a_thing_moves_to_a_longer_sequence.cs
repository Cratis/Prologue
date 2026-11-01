// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Captures;
using Cratis.Prologue.Workbench.Captures.given;

namespace Cratis.Prologue.Workbench.Explore.Journeys.for_Journeys;

public class when_a_thing_moves_to_a_longer_sequence : Specification
{
    JourneySequence[] _sequences;
    EntityTrail[] _trails;

    void Because()
    {
        IEnumerable<EntityTrail> trails = [];
        IEnumerable<EvictedEntity> evicted = [];
        IEnumerable<JourneySequence> sequences = [];
        foreach (var step in new[] { "reserve", "lend", "return" })
        {
            var command = new RecordedCommand("POST", $"/api/loans/9/{step}", 200, TraceId.None, CommandShape.From("POST", $"/api/loans/9/{step}"), 0);
            (trails, evicted, sequences) = JourneyInference.Link(trails, evicted, sequences, command);
        }

        _trails = [.. trails];
        _sequences = [.. JourneyInference.Describe(sequences)];
    }

    [Fact] void should_keep_only_where_the_thing_ended_up() => _sequences.Single().Steps.Count().ShouldEqual(3);
    [Fact] void should_keep_the_steps_in_order() => _trails.Single().Steps.Select(step => step.Value).First().ShouldEqual("POST /api/loans/{id}/reserve");
    [Fact] void should_drop_a_sequence_nothing_ends_at_anymore() => _sequences.Length.ShouldEqual(1);
}
#endif
