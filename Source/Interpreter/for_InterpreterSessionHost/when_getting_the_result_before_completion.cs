// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Interpretation;

namespace Cratis.Prologue.Interpreter.for_InterpreterSessionHost;

public class when_getting_the_result_before_completion : given.all_dependencies
{
    SessionResult? _result;

    void Establish() => SessionProceedsTo(StateWith(InterpreterStatus.AwaitingAnswers, new InterpreterQuestion(Guid.NewGuid(), "?", [], string.Empty)));

    async Task Because()
    {
        await _host.Start(_llmOptions);
        await _host.Work;
        _result = _host.GetResult();
    }

    [Fact] void should_have_no_result() => _result.ShouldBeNull();
    [Fact] void should_not_count_a_fetch() => _activity.DidNotReceive().ResultFetched();
}
#endif
