// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Interpretation.for_InterpreterSession;

public class when_the_model_responds_with_malformed_json : given.all_dependencies
{
    IInterpreterSession _session;
    InterpreterSessionState _result;

    void Establish()
    {
        RespondWith("I am terribly sorry, I cannot produce JSON today.");
        _session = _factory.CreateNew(_prologueId, _captures, _llmOptions);
    }

    async Task Because() => _result = await _session.Proceed();

    [Fact] void should_still_complete() => _result.Status.ShouldEqual(InterpreterStatus.Completed);
    [Fact] void should_fall_back_to_the_heuristic_names() => _result.Model!.Modules.Single().Features.Single().Slices.Single().Commands.Single().Name.ShouldEqual("CreateAuthor");
    [Fact] void should_derive_the_system_name_deterministically() => _result.Model!.SystemName.ShouldEqual("Api");
    [Fact] void should_have_no_pending_questions() => _result.PendingQuestions.ShouldBeEmpty();
}
#endif
