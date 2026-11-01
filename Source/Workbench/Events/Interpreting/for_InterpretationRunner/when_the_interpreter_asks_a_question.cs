// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Interpretation;
using Cratis.Prologue.Workbench.Events.Interpreting.given;

namespace Cratis.Prologue.Workbench.Events.Interpreting.for_InterpretationRunner;

public class when_the_interpreter_asks_a_question : an_interpretation_runner
{
    InterpretationProgress _waiting;
    IReadOnlyList<InterpreterQuestion> _pending;

    async Task Because()
    {
        _responses.Enqueue(AQuestion);
        _responses.Enqueue(Refinement);
        await Run();
        _waiting = Progress;
        _pending = _sessions.Pending(_prologue);
        _sessions.Answer(_prologue, _pending[0].Id, "Yes");
        await Run(resume: true);
    }

    [Fact] void should_wait_for_the_answer() => _waiting.State.ShouldEqual(InterpretationState.AwaitingAnswers);
    [Fact] void should_say_how_many_questions_wait() => _waiting.Message.ShouldContain("1 question");
    [Fact] void should_hold_the_question() => _pending.Single().Prompt.ShouldEqual("Is fulfilling a reservation a fact?");
    [Fact] void should_record_the_result_once_in_the_end() => _recorded.Count.ShouldEqual(1);
    [Fact] void should_complete_once_the_answer_is_in() => Progress.State.ShouldEqual(InterpretationState.Completed);
    [Fact] void should_apply_the_refinement_the_answer_led_to() => _recorded[0].Suggestions.Select(suggestion => suggestion.Refined.Value).ShouldContain("AuthorRegistered");
    [Fact] void should_have_no_question_left() => _sessions.Pending(_prologue).ShouldBeEmpty();
}
#endif
