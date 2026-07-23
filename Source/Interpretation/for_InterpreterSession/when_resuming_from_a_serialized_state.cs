// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Text.Json;
using Microsoft.Extensions.AI;

namespace Cratis.Prologue.Interpretation.for_InterpreterSession;

public class when_resuming_from_a_serialized_state : given.all_dependencies
{
    static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    IInterpreterSession _resumed;
    InterpreterSessionState _result;

    async Task Establish()
    {
        RespondWith(QuestionResponse, FinalResponse);

        // The first session parks awaiting answers; its state round-trips through JSON exactly like a host
        // persisting and later restoring it.
        var original = _factory.CreateNew(_prologueId, _captures, _llmOptions);
        var awaiting = await original.Proceed();
        var persisted = JsonSerializer.Deserialize<InterpreterSessionState>(JsonSerializer.Serialize(awaiting, _json), _json)!;

        _resumed = _factory.Resume(persisted, _captures, _llmOptions, _statuses.Add);
        _resumed.Answer(new InterpreterAnswer(persisted.PendingQuestions[0].Id, "No"));
    }

    async Task Because() => _result = await _resumed.Proceed();

    [Fact] void should_complete() => _result.Status.ShouldEqual(InterpreterStatus.Completed);
    [Fact] void should_count_the_completed_round() => _result.QuestionRounds.ShouldEqual(1);
    [Fact] void should_not_rebuild_the_heuristic_model() => _heuristics.Received(1).Build(Arg.Any<Guid>(), Arg.Any<IReadOnlyList<Cratis.Prologue.Contracts.Capture>>());
    [Fact] void should_replay_the_full_transcript_to_the_stateless_model() => _sentMessages[1].Count.ShouldEqual(4);
    [Fact] void should_replay_the_system_message_first() => _sentMessages[1][0].Role.ShouldEqual(ChatRole.System);
    [Fact] void should_relay_the_answer_given_after_the_resume() => _sentMessages[1][^1].Text.ShouldContain("A: No");
    [Fact] void should_apply_the_refinement() => _result.Model!.SystemName.ShouldEqual("LibrarySystem");
}
#endif
