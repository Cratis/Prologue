// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Configuration;

namespace Cratis.Prologue.Interpretation.for_InterpreterSession;

public class when_proceeding_with_the_language_model_disabled : given.all_dependencies
{
    IInterpreterSession _session;
    InterpreterSessionState _result;

    void Establish()
    {
        _llmOptions.Enabled = false;
        _session = _factory.CreateNew(_prologueId, _captures, _llmOptions, _statuses.Add);
    }

    async Task Because() => _result = await _session.Proceed();

    [Fact] void should_complete() => _result.Status.ShouldEqual(InterpreterStatus.Completed);
    [Fact] void should_skip_refining_entirely() => _statuses.ShouldNotContain(InterpreterStatus.Refining);
    [Fact] void should_report_the_stages_in_order() => _statuses.ShouldContainOnly(InterpreterStatus.ReadingCaptures, InterpreterStatus.AnalyzingEvidence, InterpreterStatus.BuildingModel, InterpreterStatus.Completed);
    [Fact] void should_never_create_a_chat_client() => _chatClients.DidNotReceive().CreateFor(Arg.Any<LlmOptions>());
    [Fact] void should_derive_the_system_name_deterministically() => _result.Model!.SystemName.ShouldEqual("Api");
    [Fact] void should_keep_the_heuristic_names() => _result.Model!.Modules.Single().Features.Single().Slices.Single().Commands.Single().Name.ShouldEqual("CreateAuthor");
    [Fact] void should_have_no_pending_questions() => _result.PendingQuestions.ShouldBeEmpty();
    [Fact] void should_have_an_empty_transcript() => _result.Transcript.ShouldBeEmpty();
}
#endif
