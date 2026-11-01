// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Interpretation.for_InterpreterSession;

public class when_refining_without_additional_evidence : given.all_dependencies
{
    InterpreterSessionState _result;

    async Task Because()
    {
        RespondWith(FinalResponse);
        _result = await _factory.CreateNew(_prologueId, _captures, _llmOptions, _statuses.Add).Proceed();
    }

    [Fact] void should_not_add_an_additional_evidence_section() => _sentMessages[0][^1].Text.ShouldNotContain("Additional evidence");
    [Fact] void should_complete() => _result.Status.ShouldEqual(InterpreterStatus.Completed);
}
#endif
