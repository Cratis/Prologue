// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Interpretation.for_InterpreterSession;

public class when_the_model_asks_a_question : given.all_dependencies
{
    IInterpreterSession _session;
    InterpreterSessionState _result;

    void Establish()
    {
        RespondWith(QuestionResponse);
        _session = _factory.CreateNew(_prologueId, _captures, _llmOptions, _statuses.Add);
    }

    async Task Because() => _result = await _session.Proceed();

    [Fact] void should_await_answers() => _result.Status.ShouldEqual(InterpreterStatus.AwaitingAnswers);
    [Fact] void should_report_refining_before_awaiting() => _statuses.ShouldContain(InterpreterStatus.Refining);
    [Fact] void should_expose_the_question() => _result.PendingQuestions.Single().Prompt.ShouldEqual("Is an author the same as a writer?");
    [Fact] void should_expose_the_context() => _result.PendingQuestions.Single().Context.ShouldEqual("The evidence uses both terms");
    [Fact] void should_expose_the_choices() => _result.PendingQuestions.Single().Choices.Count.ShouldEqual(2);
    [Fact] void should_assign_the_question_an_identity() => _result.PendingQuestions.Single().Id.ShouldNotEqual(Guid.Empty);
    [Fact] void should_not_count_the_round_until_answers_arrive() => _result.QuestionRounds.ShouldEqual(0);
    [Fact] void should_send_the_system_instructions_first() => _sentMessages.Single()[0].Text.ShouldContain("renames");
    [Fact] void should_send_the_evidence_message() => _sentMessages.Single()[1].Text.ShouldContain("POST /api/authors");
    [Fact] void should_record_the_assistant_response_in_the_transcript() => _result.Transcript[^1].Role.ShouldEqual("assistant");
}
#endif
