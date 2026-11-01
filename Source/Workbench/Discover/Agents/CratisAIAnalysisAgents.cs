// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.AI.Conversations;
using Cratis.Prologue.Workbench.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cratis.Prologue.Workbench.Discover.Agents;

/// <summary>
/// Reads batches through Cratis.AI's chat clients under the Dramaturg, Prompter and Usher purposes, so the pools and network
/// posture a person set up decide where the code is sent.
/// </summary>
/// <param name="services">Creates the scope a language model is resolved in.</param>
/// <param name="logger">The logger.</param>
public class CratisAIAnalysisAgents(IServiceProvider services, ILogger<CratisAIAnalysisAgents> logger) : IAnalysisAgents
{
    /// <inheritdoc/>
    public async Task<AgentFindings> Read(AnalysisBatch batch, CancellationToken cancellationToken)
    {
        var lines = batch.LineCounts;
        var failures = new List<string>();

        var capabilities = await Ask(PrologueAgents.Dramaturg, AnalysisPrompts.Capabilities(batch), text => AnalysisOutputParser.Capabilities(text, lines), failures, cancellationToken);
        var decisions = await Ask(PrologueAgents.Prompter, AnalysisPrompts.DecisionPoints(batch), text => AnalysisOutputParser.DecisionPoints(text, lines), failures, cancellationToken);
        var dependencies = await Ask(PrologueAgents.Usher, AnalysisPrompts.Dependencies(batch), AnalysisOutputParser.Dependencies, failures, cancellationToken);

        return new(new AnalysisFindings(capabilities, decisions, dependencies), failures);
    }

    async Task<IReadOnlyList<T>> Ask<T>(PrologueAgent agent, string prompt, Func<string, ParsedFindings<T>> parse, List<string> failures, CancellationToken cancellationToken)
    {
        string text;
        try
        {
            await using var scope = services.CreateAsyncScope();
            using var client = scope.ServiceProvider.GetRequiredService<IAIChatClients>().For(agent.Purpose);
            var response = await client.GetResponseAsync(prompt, cancellationToken: cancellationToken);
            text = response.Text;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.AgentUnavailable(agent.Name, exception.Message);
            failures.Add($"{agent.Name} could not answer: {exception.Message}");
            return [];
        }

        var parsed = parse(text);
        foreach (var reason in parsed.Dropped)
        {
            logger.EntryDropped(agent.Name, reason);
        }

        return parsed.Items;
    }
}
