// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Interpretation.for_InterpreterSession;

public class when_refining_with_additional_evidence : given.all_dependencies
{
    InterpreterSessionState _result;

    async Task Because()
    {
        RespondWith(FinalResponse);
        var session = _factory.CreateNew(_prologueId, _captures, _llmOptions, _statuses.Add, additionalEvidence: "- POST /api/authors: refused with 409 when the name is taken (AuthorRules.cs:12)");
        _result = await session.Proceed();
    }

    [Fact] void should_give_the_additional_evidence_to_the_language_model() => _sentMessages[0][^1].Text.ShouldContain("refused with 409 when the name is taken");
    [Fact] void should_name_the_additional_evidence_section() => _sentMessages[0][^1].Text.ShouldContain("Additional evidence");
    [Fact] void should_keep_the_renames_the_refinement_applied() => _result.Renames!["AuthorCreated"].ShouldEqual("AuthorRegistered");
}
#endif
