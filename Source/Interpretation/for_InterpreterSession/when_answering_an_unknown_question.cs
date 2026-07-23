// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Interpretation.for_InterpreterSession;

public class when_answering_an_unknown_question : given.all_dependencies
{
    IInterpreterSession _session;
    Exception _error;

    async Task Establish()
    {
        RespondWith(QuestionResponse);
        _session = _factory.CreateNew(_prologueId, _captures, _llmOptions);
        await _session.Proceed();
    }

    void Because() => _error = Catch.Exception(() => _session.Answer(new InterpreterAnswer(Guid.NewGuid(), "Yes")));

    [Fact] void should_throw_question_not_pending() => _error.ShouldBeOfExactType<QuestionNotPending>();
    [Fact] void should_keep_the_question_pending() => _session.State.PendingQuestions.Count.ShouldEqual(1);
    [Fact] void should_record_no_answer() => _session.State.AnsweredQuestions.ShouldBeEmpty();
}
#endif
