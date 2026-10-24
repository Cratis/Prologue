// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Interpretation;
using Cratis.Prologue.Interpreter.Contracts;

namespace Cratis.Prologue.Interpreter.for_InterpreterSessionStateDocument;

public class when_round_tripping_a_session_state : Specification
{
    static readonly Guid _prologueId = new("11111111-2222-3333-4444-555555555555");

    InterpreterSessionState _state;
    InterpreterSessionStateDocument _document;
    InterpreterSessionState? _result;

    void Establish() => _state = new InterpreterSessionState(
        _prologueId,
        InterpreterStatus.AwaitingAnswers,
        new ExtractionResult(_prologueId, [], "LibrarySystem"),
        [new InterpreterQuestion(Guid.NewGuid(), "Is an author the same as a writer?", [new QuestionChoice("Yes", string.Empty)], "The evidence uses both terms")],
        [],
        [new SessionChatMessage("system", "instructions")],
        1,
        string.Empty);

    void Because()
    {
        _document = InterpreterSessionStateDocument.For(_prologueId, _state);
        _result = _document.ToState();
    }

    [Fact] void should_key_the_document_by_the_prologue() => _document.Id.ShouldEqual(_prologueId);
    [Fact] void should_keep_the_status() => _result!.Status.ShouldEqual(InterpreterStatus.AwaitingAnswers);
    [Fact] void should_keep_the_system_name() => _result!.Model!.SystemName.ShouldEqual("LibrarySystem");
    [Fact] void should_keep_the_pending_question() => _result!.PendingQuestions.Single().Prompt.ShouldEqual("Is an author the same as a writer?");
    [Fact] void should_keep_the_transcript() => _result!.Transcript.Single().Role.ShouldEqual("system");
    [Fact] void should_keep_the_question_rounds() => _result!.QuestionRounds.ShouldEqual(1);
}
#endif
