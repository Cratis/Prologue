// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Interpretation;

namespace Cratis.Prologue.Interpreter.for_InterpreterSessionHost;

public class when_getting_the_snapshot_of_a_restored_session_before_starting : given.all_dependencies
{
    InterpreterQuestion _question;
    SessionSnapshot _snapshot;

    void Establish()
    {
        _question = new InterpreterQuestion(Guid.NewGuid(), "Is an author the same as a writer?", [], string.Empty);
        _host.Restore(StateWith(InterpreterStatus.AwaitingAnswers, _question));
    }

    void Because() => _snapshot = _host.GetSnapshot();

    [Fact] void should_reflect_the_persisted_status() => _snapshot.Status.ShouldEqual(InterpreterStatus.AwaitingAnswers);
    [Fact] void should_expose_the_persisted_pending_question() => _snapshot.PendingQuestions.Single().ShouldEqual(_question);
}
#endif
