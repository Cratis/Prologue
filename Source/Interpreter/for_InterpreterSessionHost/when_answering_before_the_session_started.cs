// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Interpretation;

namespace Cratis.Prologue.Interpreter.for_InterpreterSessionHost;

public class when_answering_before_the_session_started : given.all_dependencies
{
    Exception _error;

    async Task Because() => _error = await Catch.Exception(() => _host.Answer(new InterpreterAnswer(Guid.NewGuid(), "Yes")));

    [Fact] void should_fail_with_question_not_pending() => _error.ShouldBeOfExactType<QuestionNotPending>();
}
#endif
