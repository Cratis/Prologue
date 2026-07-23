// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Interpretation.for_InterpreterSession;

public class when_proceeding_with_zero_question_rounds : given.all_dependencies
{
    const string ResponseWithQuestionsAndRefinement = """
        {
          "systemName": "LibrarySystem",
          "renames": { "CreateAuthor": "RegisterAuthor" },
          "descriptions": {},
          "questions": [ { "prompt": "Should not have asked this", "context": "", "choices": [] } ]
        }
        """;

    IInterpreterSession _session;
    InterpreterSessionState _result;

    void Establish()
    {
        // Batch hosts run with zero question rounds: the system prompt tells the model not to ask, and even a
        // model that asks anyway is finalized with the refinement it returned instead of parking the session.
        RespondWith(ResponseWithQuestionsAndRefinement);
        _session = _factory.CreateNew(_prologueId, _captures, _llmOptions, maxQuestionRounds: 0);
    }

    async Task Because() => _result = await _session.Proceed();

    [Fact] void should_complete_without_awaiting_answers() => _result.Status.ShouldEqual(InterpreterStatus.Completed);
    [Fact] void should_have_no_pending_questions() => _result.PendingQuestions.ShouldBeEmpty();
    [Fact] void should_tell_the_model_not_to_ask() => _sentMessages.Single()[0].Text.ShouldContain("Do not ask questions");
    [Fact] void should_apply_the_refinement_the_model_returned() => _result.Model!.Modules.Single().Features.Single().Slices.Single().Commands.Single().Name.ShouldEqual("RegisterAuthor");
    [Fact] void should_apply_the_system_name() => _result.Model!.SystemName.ShouldEqual("LibrarySystem");
}
#endif
