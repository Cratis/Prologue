// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;

// The interaction service — how a dashboard command asks the operator a question — is still experimental in
// Aspire 13.2. It is the only way to make the command interactive, which is what the sample wants.
#pragma warning disable ASPIREINTERACTION001

namespace Library.Composition;

/// <summary>
/// Adds the dashboard buttons that drive the library's simulated load. Clicking one asks how many transactions to
/// run and then posts to the Core service, so a system under realistic use can be produced without leaving the
/// Aspire dashboard.
/// </summary>
public static class SimulationCommands
{
    /// <summary>
    /// Adds the start and stop simulation commands to the Core service.
    /// </summary>
    /// <param name="core">The Core service the commands act on.</param>
    /// <param name="defaultTransactionCount">The transaction count offered when the operator is asked.</param>
    /// <returns>The <paramref name="core"/> builder for chaining.</returns>
    public static IResourceBuilder<ProjectResource> WithSimulationCommands(
        this IResourceBuilder<ProjectResource> core,
        int defaultTransactionCount)
    {
        core.WithCommand(
            name: "simulate-load",
            displayName: "Simulate load",
            executeCommand: context => StartSimulation(core, defaultTransactionCount, context),
            commandOptions: new CommandOptions
            {
                Description = "Runs a burst of realistic library traffic through the Prologue Extractor.",
                IconName = "Play",
                IconVariant = IconVariant.Filled,
                IsHighlighted = true,
                UpdateState = OnlyWhenRunning
            });

        core.WithCommand(
            name: "stop-simulation",
            displayName: "Stop simulation",
            executeCommand: context => StopSimulation(core, context),
            commandOptions: new CommandOptions
            {
                Description = "Stops the simulation run in flight.",
                IconName = "Stop",
                UpdateState = OnlyWhenRunning
            });

        return core;
    }

    static ResourceCommandState OnlyWhenRunning(UpdateCommandStateContext context) =>
        context.ResourceSnapshot.State?.Text == KnownResourceStates.Running
            ? ResourceCommandState.Enabled
            : ResourceCommandState.Disabled;

    static async Task<ExecuteCommandResult> StartSimulation(
        IResourceBuilder<ProjectResource> core,
        int defaultTransactionCount,
        ExecuteCommandContext context)
    {
        var interaction = context.ServiceProvider.GetRequiredService<IInteractionService>();

        var transactionCount = defaultTransactionCount;

        if (interaction.IsAvailable)
        {
            var answer = await interaction.PromptInputAsync(
                title: "Simulate a system under use",
                message: "How many transactions should the library carry out? Each one is a real HTTP command through the Prologue Extractor.",
                input: new InteractionInput
                {
                    Name = "transactionCount",
                    Label = "Transactions",
                    InputType = InputType.Number,
                    Required = true,
                    Value = defaultTransactionCount.ToString(CultureInfo.InvariantCulture)
                },
                cancellationToken: context.CancellationToken);

            if (answer.Canceled)
            {
                return CommandResults.Canceled();
            }

            if (!int.TryParse(answer.Data.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out transactionCount) ||
                transactionCount <= 0)
            {
                return CommandResults.Failure("Enter a transaction count greater than zero.");
            }
        }

        return await Post(core, "api/simulation/start", new { transactionCount }, context);
    }

    static Task<ExecuteCommandResult> StopSimulation(IResourceBuilder<ProjectResource> core, ExecuteCommandContext context) =>
        Post(core, "api/simulation/stop", new { }, context);

    static async Task<ExecuteCommandResult> Post(
        IResourceBuilder<ProjectResource> core,
        string path,
        object body,
        ExecuteCommandContext context)
    {
        try
        {
            var endpoint = core.GetEndpoint(LibraryComposition.HttpEndpointName);
            using var client = new HttpClient { BaseAddress = new Uri(endpoint.Url.TrimEnd('/') + "/") };
            using var response = await client.PostAsJsonAsync(path, body, context.CancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return CommandResults.Success();
            }

            var detail = await response.Content.ReadAsStringAsync(context.CancellationToken);
            return CommandResults.Failure($"The library answered {(int)response.StatusCode}: {detail}");
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return CommandResults.Failure(exception);
        }
    }
}
