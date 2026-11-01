// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Captures;

namespace Cratis.Prologue.Workbench.Explore.Journeys.for_Journeys;

public class when_there_are_more_sequences_than_are_kept : Specification
{
    JourneySequence[] _sequences;
    JourneySequence _other;

    static RecordedCommand Step(int thing, string step) =>
        new("POST", $"/api/loans/{thing}/{step}", 200, TraceId.None, CommandShape.From("POST", $"/api/loans/{thing}/{step}"), 0);

    // A step name with a digit would be generalized to {id}; letters keep every step a shape of its own.
    static string Letters(int number) => new([.. number.ToString(System.Globalization.CultureInfo.InvariantCulture).Select(digit => (char)('a' + (digit - '0')))]);

    void Because()
    {
        IEnumerable<EntityTrail> trails = [];
        IEnumerable<EvictedEntity> evicted = [];
        IEnumerable<JourneySequence> sequences = [];

        // Every thing goes through a sequence of its own: lend, then a step that only it takes.
        const int things = JourneyInference.SequencesKept + 30;
        for (var thing = 0; thing < things; thing++)
        {
            (trails, evicted, sequences) = JourneyInference.Link(trails, evicted, sequences, Step(thing, "lend"));
            (trails, evicted, sequences) = JourneyInference.Link(trails, evicted, sequences, Step(thing, Letters(thing)));
        }

        _sequences = [.. JourneyInference.Describe(sequences)];
        _other = _sequences.Single(sequence => !sequence.Steps.Any());
    }

    [Fact] void should_keep_no_more_than_the_limit_and_one_for_the_rest() => _sequences.Length.ShouldEqual(JourneyInference.SequencesKept + 1);
    [Fact] void should_count_the_things_of_the_sequences_that_are_not_listed() => _other.Entities.ShouldEqual(30);
    [Fact] void should_not_say_the_others_went_on() => _other.ContinuedBy.IsNone.ShouldBeTrue();
    [Fact] void should_count_every_thing_once() => _sequences.Sum(sequence => sequence.Entities).ShouldEqual(JourneyInference.SequencesKept + 30);
}
#endif
