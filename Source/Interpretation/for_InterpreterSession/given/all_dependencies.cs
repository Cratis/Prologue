// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Configuration;
using Cratis.Prologue.Contracts;
using Cratis.Prologue.Interpreter.Contracts;
using Microsoft.Extensions.AI;

namespace Cratis.Prologue.Interpretation.for_InterpreterSession.given;

public class all_dependencies : Specification
{
    protected const string QuestionResponse = """
        {
          "systemName": "",
          "renames": {},
          "descriptions": {},
          "questions": [
            {
              "prompt": "Is an author the same as a writer?",
              "context": "The evidence uses both terms",
              "choices": [
                { "label": "Yes", "description": "They are the same concept" },
                { "label": "No", "description": "" }
              ]
            }
          ]
        }
        """;

    protected const string FinalResponse = """
        {
          "systemName": "LibrarySystem",
          "renames": { "Api": "Library", "CreateAuthor": "RegisterAuthor", "AuthorCreated": "AuthorRegistered" },
          "descriptions": {
            "module:Library": "Everything about the library",
            "feature:Library/Authors": "The author lifecycle",
            "slice:Library/Authors/Create": "Registers an author"
          },
          "questions": []
        }
        """;

    protected static readonly Guid _prologueId = new("11111111-2222-3333-4444-555555555555");

    protected IBuildHeuristicModel _heuristics;
    protected IChatClient _chatClient;
    protected IChatClientFactory _chatClients;
    protected ILogger<InterpreterSession> _logger;
    protected LlmOptions _llmOptions;
    protected List<InterpreterStatus> _statuses;
    protected InterpreterSessionFactory _factory;
    protected ExtractionResult _heuristicModel;
    protected List<Capture> _captures;
    protected List<IReadOnlyList<ChatMessage>> _sentMessages;
    protected Queue<string> _responses;

    void Establish()
    {
        _heuristics = Substitute.For<IBuildHeuristicModel>();
        _chatClient = Substitute.For<IChatClient>();
        _chatClients = Substitute.For<IChatClientFactory>();
        _chatClients.CreateFor(Arg.Any<LlmOptions>()).Returns(_chatClient);
        _logger = Substitute.For<ILogger<InterpreterSession>>();
        _llmOptions = new LlmOptions { Enabled = true };
        _statuses = [];
        _sentMessages = [];
        _responses = new Queue<string>();
        _factory = new InterpreterSessionFactory(_heuristics, _chatClients, _logger);

        _heuristicModel = new ExtractionResult(
            _prologueId,
            [
                new ExtractedModule(
                    "Api",
                    [
                        new ExtractedFeature(
                            "Authors",
                            [],
                            [
                                new ExtractedSlice(
                                    "Create",
                                    ExtractedSliceType.StateChange,
                                    [new ExtractedCommand("CreateAuthor", [new ExtractedProperty("Name", "string")], [])],
                                    [new ExtractedEvent("AuthorCreated", [new ExtractedProperty("Name", "string")])],
                                    [],
                                    [],
                                    [])
                            ])
                    ])
            ]);
        _heuristics.Build(Arg.Any<Guid>(), Arg.Any<IReadOnlyList<Capture>>()).Returns(_heuristicModel);

        var occurred = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        _captures =
        [
            new Capture(
                Guid.NewGuid(),
                occurred,
                [new Observation(SourceKind.Http, occurred, new HttpCommandObserved("POST", "/api/authors", 201))],
                _prologueId)
        ];

        _chatClient
            .GetResponseAsync(Arg.Any<IEnumerable<ChatMessage>>(), Arg.Any<ChatOptions?>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                _sentMessages.Add([.. callInfo.Arg<IEnumerable<ChatMessage>>()]);
                return new ChatResponse([new ChatMessage(ChatRole.Assistant, _responses.Dequeue())]);
            });
    }

    protected void RespondWith(params string[] responses)
    {
        foreach (var response in responses)
        {
            _responses.Enqueue(response);
        }
    }
}
#endif
