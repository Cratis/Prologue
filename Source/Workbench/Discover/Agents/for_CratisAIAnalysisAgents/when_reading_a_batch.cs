// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.AI.Conversations;
using Cratis.AI.LanguageModels;
using Cratis.Prologue.Workbench.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Prologue.Workbench.Discover.Agents.for_CratisAIAnalysisAgents;

public class when_reading_a_batch : Specification
{
    IAIChatClients _clients;
    AgentFindings _findings;
    AgentFindings _unavailable;
    string _prompt;

    AnalysisBatch Batch => new([new BatchFile(new SourcePath("Loans/LoansController.cs"), string.Join('\n', Enumerable.Range(1, 100).Select(_ => $"line {_}")))]);

    static IChatClient Answering(string text, Action<string>? seen = null)
    {
        var client = Substitute.For<IChatClient>();
        client.GetResponseAsync(Arg.Any<IEnumerable<ChatMessage>>(), Arg.Any<ChatOptions?>(), Arg.Any<CancellationToken>()).Returns(call =>
        {
            seen?.Invoke(call.Arg<IEnumerable<ChatMessage>>().Last().Text);
            return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, text)));
        });
        return client;
    }

    void Establish()
    {
        var dramaturg = Answering(
            """{"capabilities":[{"module":"Loans","capability":"Lend a copy","commands":["POST /api/loans"],"file":"Loans/LoansController.cs"}]}""",
            prompt => _prompt = prompt);
        var prompter = Answering(
            """{"decisionPoints":[{"rule":"Refuse a loan when no copy is available","file":"Loans/LoansController.cs","line":71,"kind":"refusal"},{"rule":"Off the end","file":"Loans/LoansController.cs","line":900,"kind":"refusal"}]}""");
        var usher = Answering("not json at all");

        _clients = Substitute.For<IAIChatClients>();
        _clients.For(PrologueAgents.Dramaturg.Purpose).Returns(dramaturg);
        _clients.For(PrologueAgents.Prompter.Purpose).Returns(prompter);
        _clients.For(PrologueAgents.Usher.Purpose).Returns(usher);
    }

    async Task Because()
    {
        var agents = new CratisAIAnalysisAgents(new ServiceCollection().AddSingleton(_clients).BuildServiceProvider().GetRequiredService<IServiceScopeFactory>(), Microsoft.Extensions.Logging.Abstractions.NullLogger<CratisAIAnalysisAgents>.Instance);
        _findings = await agents.Read(Batch, CancellationToken.None);

        var down = Substitute.For<IAIChatClients>();
        down.For(Arg.Any<LanguageModelPurpose>()).Returns(_ => throw new InvalidOperationException("No pool is configured for the agent"));
        _unavailable = await new CratisAIAnalysisAgents(new ServiceCollection().AddSingleton(down).BuildServiceProvider().GetRequiredService<IServiceScopeFactory>(), Microsoft.Extensions.Logging.Abstractions.NullLogger<CratisAIAnalysisAgents>.Instance).Read(Batch, CancellationToken.None);
    }

    [Fact] void should_ask_the_dramaturg_for_capabilities() => _findings.Findings.Capabilities.Single().Capability.Value.ShouldEqual("Lend a copy");
    [Fact] void should_ask_the_prompter_for_decision_points_and_drop_the_ones_off_the_end() => _findings.Findings.DecisionPoints.Single().Line.Value.ShouldEqual(71);
    [Fact] void should_keep_an_unusable_answer_from_the_usher_out() => _findings.Findings.Dependencies.ShouldBeEmpty();
    [Fact] void should_have_no_failures_when_every_agent_answered() => _findings.Failures.ShouldBeEmpty();
    [Fact] void should_show_the_agent_numbered_lines() => _prompt.ShouldContain("71: line 71");
    [Fact] void should_show_the_agent_the_file_header() => _prompt.ShouldContain("=== Loans/LoansController.cs ===");
    [Fact] void should_ask_for_json_only() => _prompt.ShouldContain("JSON only");
    [Fact] void should_say_why_no_agent_answered() => _unavailable.Failures.Count.ShouldEqual(3);
    [Fact] void should_name_the_agent_that_could_not_answer() => _unavailable.Failures.Any(_ => _.StartsWith("Prompter could not answer", StringComparison.Ordinal)).ShouldBeTrue();
    [Fact] void should_find_nothing_without_a_provider() => _unavailable.Findings.Capabilities.ShouldBeEmpty();
}
#endif
