// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Events.Interpreting.for_InterpretationQueue;

public class when_queueing : Specification
{
    PrologueId _prologue;
    PrologueId _other;
    InterpretationProgressStore _progress;
    InterpretationQueue _queue;
    bool _first;
    bool _second;
    bool _otherPrologue;
    bool _afterDone;
    InterpretationJob _next;

    void Establish()
    {
        _prologue = PrologueId.New();
        _other = PrologueId.New();
        _progress = new InterpretationProgressStore();
        _queue = new InterpretationQueue(_progress);
    }

    async Task Because()
    {
        _first = _queue.Enqueue(new InterpretationJob(_prologue, false));
        _second = _queue.Enqueue(new InterpretationJob(_prologue, false));
        _otherPrologue = _queue.Enqueue(new InterpretationJob(_other, false));
        _next = await _queue.Next(CancellationToken.None);
        _queue.Done(_prologue);
        _afterDone = _queue.Enqueue(new InterpretationJob(_prologue, true));
    }

    [Fact] void should_queue_the_first_request() => _first.ShouldBeTrue();
    [Fact] void should_not_queue_a_prologue_twice() => _second.ShouldBeFalse();
    [Fact] void should_queue_another_prologue() => _otherPrologue.ShouldBeTrue();
    [Fact] void should_hand_out_in_the_order_queued() => _next.Prologue.ShouldEqual(_prologue);
    [Fact] void should_queue_again_once_it_is_done() => _afterDone.ShouldBeTrue();
    [Fact] void should_say_it_waits_for_its_turn() => _progress.Get(_other).State.ShouldEqual(InterpretationState.Queued);
}
#endif
