// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Interpretation;
using Cratis.Prologue.Interpreter.Contracts;

namespace Cratis.Prologue.Interpreter.for_SessionSnapshot;

public class when_mapping_a_state_with_a_model : Specification
{
    static readonly Guid _prologueId = new("11111111-2222-3333-4444-555555555555");

    InterpreterQuestion _question;
    InterpreterSessionState _state;
    SessionSnapshot _result;

    void Establish()
    {
        _question = new InterpreterQuestion(Guid.NewGuid(), "Is an author the same as a writer?", [], "The evidence uses both terms");
        _state = InterpreterSessionState.New(_prologueId) with
        {
            Status = InterpreterStatus.AwaitingAnswers,
            Model = new ExtractionResult(_prologueId, [], "LibrarySystem"),
            PendingQuestions = [_question]
        };
    }

    void Because() => _result = SessionSnapshot.From(_state);

    [Fact] void should_carry_the_status() => _result.Status.ShouldEqual(InterpreterStatus.AwaitingAnswers);
    [Fact] void should_carry_the_pending_questions() => _result.PendingQuestions.Single().ShouldEqual(_question);
    [Fact] void should_carry_the_system_name() => _result.SystemName.ShouldEqual("LibrarySystem");
    [Fact] void should_carry_no_error() => _result.Error.ShouldEqual(string.Empty);
}
#endif
