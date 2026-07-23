// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Interpretation.for_InterpreterSession;

public class when_the_model_keeps_asking_beyond_the_round_limit : given.all_dependencies
{
    const string InsatiableResponse = """
        {
          "systemName": "LibrarySystem",
          "renames": { "CreateAuthor": "RegisterAuthor" },
          "descriptions": {},
          "questions": [ { "prompt": "One more thing?", "context": "", "choices": [] } ]
        }
        """;

    IInterpreterSession _session;
    InterpreterSessionState _result;

    void Establish()
    {
        RespondWith(QuestionResponse, QuestionResponse, QuestionResponse, InsatiableResponse);
        _session = _factory.CreateNew(_prologueId, _captures, _llmOptions);
    }

    async Task Because()
    {
        var state = await _session.Proceed();
        while (state.Status == InterpreterStatus.AwaitingAnswers)
        {
            foreach (var question in state.PendingQuestions.ToList())
            {
                _session.Answer(new InterpreterAnswer(question.Id, "Yes"));
            }

            state = await _session.Proceed();
        }

        _result = state;
    }

    [Fact] void should_stop_after_three_rounds() => _result.QuestionRounds.ShouldEqual(3);
    [Fact] void should_finalize_with_the_last_refinement_despite_its_questions() => _result.Status.ShouldEqual(InterpreterStatus.Completed);
    [Fact] void should_apply_the_last_refinement() => _result.Model!.Modules.Single().Features.Single().Slices.Single().Commands.Single().Name.ShouldEqual("RegisterAuthor");
    [Fact] void should_have_exchanged_four_times() => _sentMessages.Count.ShouldEqual(4);
    [Fact] void should_have_no_pending_questions() => _result.PendingQuestions.ShouldBeEmpty();
}
#endif
