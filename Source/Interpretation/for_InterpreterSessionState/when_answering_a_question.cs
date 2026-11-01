// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Interpretation.for_InterpreterSessionState;

public class when_answering_a_question : Specification
{
    static readonly InterpreterQuestion _first = new(Guid.NewGuid(), "Is an author a writer?", [], "Both words are used");
    static readonly InterpreterQuestion _second = new(Guid.NewGuid(), "Is a loan a checkout?", [], "Both words are used");
    InterpreterSessionState _state;
    InterpreterSessionState _answered;
    Exception _unknown;

    void Establish() => _state = InterpreterSessionState.New(Guid.NewGuid()) with { PendingQuestions = [_first, _second] };

    void Because()
    {
        _answered = _state.Answered(new InterpreterAnswer(_first.Id, "Yes"));
        _unknown = Catch.Exception(() => _state.Answered(new InterpreterAnswer(Guid.NewGuid(), "Yes")));
    }

    [Fact] void should_leave_the_other_question_pending() => _answered.PendingQuestions.Single().ShouldEqual(_second);
    [Fact] void should_record_the_answer() => _answered.AnsweredQuestions.Single().Answer.ShouldEqual("Yes");
    [Fact] void should_leave_the_original_state_untouched() => _state.PendingQuestions.Count.ShouldEqual(2);
    [Fact] void should_refuse_a_question_that_is_not_pending() => _unknown.ShouldBeOfExactType<QuestionNotPending>();
}
#endif
