// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.AI;
using Cratis.Prologue.Workbench.AI.Asking;
using Cratis.Prologue.Workbench.Explore.given;

namespace Cratis.Prologue.Workbench.Explore.Asking.for_CaptureAnswers;

public class when_asking_about_the_captures : a_lending_library
{
    CaptureAnswer _answered;
    CaptureAnswer _uncited;
    CaptureAnswer _unavailable;
    CaptureAnswer _nothingCaptured;
    string _promptSent;
    PrologueAgent _agentAsked;

    async Task Because()
    {
        var asker = Substitute.For<IAgentAsker>();
        asker.Ask(Arg.Any<PrologueAgent>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(call =>
        {
            _agentAsked = call.ArgAt<PrologueAgent>(0);
            _promptSent = call.ArgAt<string>(1);
            return AgentReply.Answered("""{"answer": "Lending inserts a loan.", "citations": ["POST /api/loans", "POST /api/never/seen"]}""");
        });
        _answered = await CaptureAnswers.Compose("What happens when a book is lent?", _matrix, _uncorrelated, null, _scope, asker, CancellationToken.None);

        asker.Ask(Arg.Any<PrologueAgent>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(AgentReply.Answered("""{"answer": "It depends.", "citations": ["POST /api/never/seen"]}"""));
        _uncited = await CaptureAnswers.Compose("Anything?", _matrix, _uncorrelated, null, _scope, asker, CancellationToken.None);

        asker.Ask(Arg.Any<PrologueAgent>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(AgentReply.Unavailable("No AI provider is configured for the Interpreter"));
        _unavailable = await CaptureAnswers.Compose("Anything?", _matrix, _uncorrelated, null, _scope, asker, CancellationToken.None);

        _nothingCaptured = await CaptureAnswers.Compose("Anything?", null, null, null, null, asker, CancellationToken.None);
    }

    [Fact] void should_ask_the_interpreter() => _agentAsked.ShouldEqual(PrologueAgents.Interpreter);
    [Fact] void should_send_the_question() => _promptSent.ShouldContain("Question: What happens when a book is lent?");
    [Fact] void should_send_the_evidence_summary() => _promptSent.ShouldContain("- POST /api/loans | 5 |");
    [Fact] void should_not_send_what_is_excluded() => _promptSent.ShouldNotContain("/api/simulation");
    [Fact] void should_answer_with_the_citations_that_exist() => (_answered.Status, _answered.Citations.Single().Value).ShouldEqual((AnswerStatus.Answered, "POST /api/loans"));
    [Fact] void should_not_show_an_answer_that_cites_no_captured_shape() => (_uncited.Status, _uncited.Text).ShouldEqual((AnswerStatus.Uncited, string.Empty));
    [Fact] void should_say_why_there_is_no_answer_when_the_agent_cannot_be_asked() => (_unavailable.Status, _unavailable.Reason).ShouldEqual((AnswerStatus.Unavailable, "No AI provider is configured for the Interpreter"));
    [Fact] void should_not_ask_when_nothing_was_captured() => _nothingCaptured.Status.ShouldEqual(AnswerStatus.NothingCaptured);
}
#endif
