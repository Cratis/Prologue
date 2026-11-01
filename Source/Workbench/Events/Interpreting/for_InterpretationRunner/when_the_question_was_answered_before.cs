// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Events.Interpreting.given;
using Cratis.Prologue.Workbench.Events.Questions;

namespace Cratis.Prologue.Workbench.Events.Interpreting.for_InterpretationRunner;

public class when_the_question_was_answered_before : an_interpretation_runner
{
    async Task Because()
    {
        _answers.Add(new QuestionAnswer(QuestionKeys.For("Is fulfilling a reservation a fact?"), "Is fulfilling a reservation a fact?", "Yes"));
        _responses.Enqueue(AQuestion);
        _responses.Enqueue(Refinement);
        await Run();
    }

    [Fact] void should_not_ask_again() => _sessions.Pending(_prologue).ShouldBeEmpty();
    [Fact] void should_complete_on_its_own() => Progress.State.ShouldEqual(InterpretationState.Completed);
    [Fact] void should_record_the_result() => _recorded.Count.ShouldEqual(1);
}
#endif
