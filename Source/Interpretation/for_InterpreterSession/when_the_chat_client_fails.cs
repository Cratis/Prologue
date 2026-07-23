// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Microsoft.Extensions.AI;
using NSubstitute.ExceptionExtensions;

namespace Cratis.Prologue.Interpretation.for_InterpreterSession;

public class when_the_chat_client_fails : given.all_dependencies
{
    IInterpreterSession _session;
    InterpreterSessionState _result;

    void Establish()
    {
        _chatClient
            .GetResponseAsync(Arg.Any<IEnumerable<ChatMessage>>(), Arg.Any<ChatOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new EmptyLlmResponse(new Uri("http://llm:11434/api/chat")));
        _session = _factory.CreateNew(_prologueId, _captures, _llmOptions, _statuses.Add);
    }

    async Task Because() => _result = await _session.Proceed();

    [Fact] void should_never_fail_the_session() => _result.Status.ShouldEqual(InterpreterStatus.Completed);
    [Fact] void should_report_refining_before_falling_back() => _statuses.ShouldContain(InterpreterStatus.Refining);
    [Fact] void should_fall_back_to_the_heuristic_model() => _result.Model!.Modules.Single().Features.Single().Slices.Single().Commands.Single().Name.ShouldEqual("CreateAuthor");
    [Fact] void should_derive_the_system_name_deterministically() => _result.Model!.SystemName.ShouldEqual("Api");
    [Fact] void should_carry_no_error() => _result.Error.ShouldEqual(string.Empty);
}
#endif
