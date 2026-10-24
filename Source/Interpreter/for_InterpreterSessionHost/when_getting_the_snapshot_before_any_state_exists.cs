// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Interpretation;

namespace Cratis.Prologue.Interpreter.for_InterpreterSessionHost;

public class when_getting_the_snapshot_before_any_state_exists : given.all_dependencies
{
    SessionSnapshot _snapshot;

    void Because() => _snapshot = _host.GetSnapshot();

    [Fact] void should_not_have_started() => _snapshot.Status.ShouldEqual(InterpreterStatus.NotStarted);
    [Fact] void should_have_no_pending_questions() => _snapshot.PendingQuestions.ShouldBeEmpty();
}
#endif
