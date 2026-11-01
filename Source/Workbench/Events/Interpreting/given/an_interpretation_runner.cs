// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Execution;
using Cratis.Prologue.Configuration;
using Cratis.Prologue.Interpretation;
using Cratis.Prologue.Workbench.Events.given;
using Cratis.Prologue.Workbench.Events.Questions;
using Cratis.Prologue.Workbench.Prologues;
using Microsoft.Extensions.AI;

namespace Cratis.Prologue.Workbench.Events.Interpreting.given;

/// <summary>
/// A runner interpreting the library captures with the real heuristics, a command pipeline that records what it is asked to execute, and a
/// language model that answers what the specs script.
/// </summary>
public class an_interpretation_runner : a_library_interpretation
{
    protected const string Refinement = """
        { "systemName": "Library", "renames": { "AuthorCreated": "AuthorRegistered" }, "descriptions": {}, "questions": [] }
        """;

    protected const string AQuestion = """
        { "systemName": "", "renames": {}, "descriptions": {}, "questions": [ { "prompt": "Is fulfilling a reservation a fact?", "context": "The reservation is deleted with the loan", "choices": [ { "label": "Yes", "description": "" }, { "label": "No", "description": "" } ] } ] }
        """;

    protected InterpretationProgressStore _progress;
    protected InterpretationSessions _sessions;
    protected IEvidenceSource _evidence;
    protected ICommandPipeline _pipeline;
    protected IChatClient _chat;
    protected FakeChatClients _chatClients;
    protected List<RecordInterpretation> _recorded;
    protected List<string> _sent;
    protected List<QuestionAnswer> _answers;
    protected Queue<string> _responses;
    protected CommandResult _recordResult;
    protected InterpretationRunner _runner;

    protected InterpretationProgress Progress => _progress.Get(_prologue);

    void Establish()
    {
        _progress = new InterpretationProgressStore();
        _sessions = new InterpretationSessions();
        _answers = [];
        _responses = new Queue<string>();
        _recorded = [];
        _sent = [];
        _recordResult = CommandResult.Success(CorrelationId.New());
        _evidence = Substitute.For<IEvidenceSource>();
        _evidence.Load(Arg.Any<PrologueId>(), Arg.Any<CancellationToken>()).Returns(_ => Task.FromResult(new InterpretationInput(_captures, _scope, _facts, _answers)));
        _pipeline = Substitute.For<ICommandPipeline>();
        _pipeline.Execute(Arg.Any<object>(), Arg.Any<ValidationResultSeverity?>()).Returns(call =>
        {
            _recorded.Add((RecordInterpretation)call.Arg<object>());
            return Task.FromResult(_recordResult);
        });

        _chat = Substitute.For<IChatClient>();
        _chat.GetResponseAsync(Arg.Any<IEnumerable<ChatMessage>>(), Arg.Any<ChatOptions?>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                _sent.Add(call.Arg<IEnumerable<ChatMessage>>().Last().Text);
                return Task.FromResult(new ChatResponse([new ChatMessage(ChatRole.Assistant, _responses.Dequeue())]));
            });
        _chatClients = new FakeChatClients(_chat);
        _runner = Build();
    }

    protected InterpretationRunner Build() => new(
        _evidence,
        _heuristics,
        _chatClients,
        _sessions,
        _progress,
        _pipeline,
        Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance,
        Microsoft.Extensions.Logging.Abstractions.NullLogger<InterpretationRunner>.Instance);

    protected Task Run(bool resume = false) => _runner.Run(new InterpretationJob(_prologue, resume), CancellationToken.None);

    protected class FakeChatClients(IChatClient chat) : IChatClientFactory, ILanguageModelWatch
    {
        public string Failure { get; set; } = string.Empty;

        public string Unreachable { get; set; } = string.Empty;

        public IChatClient CreateFor(LlmOptions options)
        {
            if (Unreachable.Length > 0)
            {
                Failure = Unreachable;
                throw new InvalidOperationException(Unreachable);
            }

            return chat;
        }
    }
}
#endif
