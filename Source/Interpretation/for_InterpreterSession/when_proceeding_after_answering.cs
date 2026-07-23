// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Interpretation.for_InterpreterSession;

public class when_proceeding_after_answering : given.all_dependencies
{
    IInterpreterSession _session;
    InterpreterSessionState _result;

    async Task Establish()
    {
        RespondWith(QuestionResponse, FinalResponse);
        _session = _factory.CreateNew(_prologueId, _captures, _llmOptions, _statuses.Add);
        var awaiting = await _session.Proceed();
        _session.Answer(new InterpreterAnswer(awaiting.PendingQuestions[0].Id, "Yes"));
    }

    async Task Because() => _result = await _session.Proceed();

    [Fact] void should_complete() => _result.Status.ShouldEqual(InterpreterStatus.Completed);
    [Fact] void should_count_the_completed_round() => _result.QuestionRounds.ShouldEqual(1);
    [Fact] void should_record_the_answered_question() => _result.AnsweredQuestions.Single().Answer.ShouldEqual("Yes");
    [Fact] void should_have_no_pending_questions() => _result.PendingQuestions.ShouldBeEmpty();
    [Fact] void should_resend_the_full_transcript() => _sentMessages[1].Count.ShouldEqual(4);
    [Fact] void should_relay_the_answers_in_the_last_message() => _sentMessages[1][^1].Text.ShouldContain("- Q: Is an author the same as a writer?");
    [Fact] void should_relay_the_answer_value() => _sentMessages[1][^1].Text.ShouldContain("A: Yes");
    [Fact] void should_apply_the_system_name() => _result.Model!.SystemName.ShouldEqual("LibrarySystem");
    [Fact] void should_apply_the_renames() => Slice.Commands.Single().Name.ShouldEqual("RegisterAuthor");
    [Fact] void should_apply_the_module_description() => _result.Model!.Modules.Single().Description.ShouldEqual("Everything about the library");
    [Fact] void should_apply_the_slice_description() => Slice.Description.ShouldEqual("Registers an author");

    Interpreter.Contracts.ExtractedSlice Slice => _result.Model!.Modules.Single().Features.Single().Slices.Single();
}
#endif
