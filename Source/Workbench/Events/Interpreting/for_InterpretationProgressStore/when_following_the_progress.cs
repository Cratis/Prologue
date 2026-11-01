// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Events.Interpreting.for_InterpretationProgressStore;

public class when_following_the_progress : Specification
{
    PrologueId _prologue;
    InterpretationProgressStore _store;
    List<InterpretationProgress> _seen;
    InterpretationProgress _neverRun;

    void Establish()
    {
        _prologue = PrologueId.New();
        _store = new InterpretationProgressStore();
        _seen = [];
    }

    void Because()
    {
        using var subscription = _store.Observe(_prologue).Subscribe(_seen.Add);
        _store.Set(new InterpretationProgress(_prologue, InterpretationState.Running, "Reading the captures", false, 0));
        _neverRun = _store.Get(PrologueId.New());
    }

    [Fact] void should_start_with_idle() => _seen[0].State.ShouldEqual(InterpretationState.Idle);
    [Fact] void should_push_the_change() => _seen[1].Message.ShouldEqual("Reading the captures");
    [Fact] void should_remember_the_latest() => _store.Get(_prologue).State.ShouldEqual(InterpretationState.Running);
    [Fact] void should_be_idle_for_a_prologue_nothing_ran_for() => _neverRun.State.ShouldEqual(InterpretationState.Idle);
}
#endif
