// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Cratis.AI.Conversations;
using Cratis.AI.LanguageModels;
using Microsoft.Extensions.AI;

namespace Cratis.Prologue.Workbench.AI.Dispatch;

/// <summary>
/// A chat client that, on every call, sends an agent's work to the members of its pool in the order the Workbench recorded, under the pool's
/// fallback and daily ceiling. Each member is reached through Cratis.AI's chat client for that one provider, so its concurrency limit, rate-limit
/// cooldown and usage recording still apply. An agent that does not draw from a pool is left to Cratis.AI.
/// </summary>
/// <param name="purpose">The purpose, the agent's identity.</param>
/// <param name="inner">Cratis.AI's chat clients.</param>
/// <param name="plans">Plans the dispatch of each call.</param>
/// <param name="spend">Holds the pool's use for the day and records what each answer cost.</param>
public class DispatchingChatClient(LanguageModelPurpose purpose, IAIChatClients inner, IPoolPlans plans, IPoolSpendLedger spend) : IChatClient
{
    /// <inheritdoc/>
    public async Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        var chat = messages.ToList();
        var plan = await plans.For(purpose, cancellationToken);
        if (plan is null)
        {
            using var direct = inner.For(purpose);
            return await direct.GetResponseAsync(chat, options, cancellationToken);
        }

        using var reservation = spend.Reserve(plan);
        var response = await PoolWalker.Walk(
            plan with { Used = reservation.UsedBefore },
            async member =>
            {
                using var client = inner.For(purpose, plan.Agent with { ProviderId = member.Provider });
                return await client.GetResponseAsync(chat, options, cancellationToken);
            },
            cancellationToken);
        reservation.Settle(response.Usage);
        return response;
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var chat = messages.ToList();
        var plan = await plans.For(purpose, cancellationToken);
        if (plan is null)
        {
            using var direct = inner.For(purpose);
            await foreach (var update in direct.GetStreamingResponseAsync(chat, options, cancellationToken))
            {
                yield return update;
            }

            yield break;
        }

        // A member can only be passed over until its first update has arrived; after that the vendor has started answering.
        using var reservation = spend.Reserve(plan);
        var started = await PoolWalker.Walk(
            plan with { Used = reservation.UsedBefore },
            async member =>
            {
                var client = inner.For(purpose, plan.Agent with { ProviderId = member.Provider });
                var updates = client.GetStreamingResponseAsync(chat, options, cancellationToken).GetAsyncEnumerator(cancellationToken);
                try
                {
                    return new StartedStream(client, updates, await updates.MoveNextAsync());
                }
                catch
                {
                    await updates.DisposeAsync();
                    client.Dispose();
                    throw;
                }
            },
            cancellationToken);

        var usage = new UsageDetails();
        try
        {
            var hasUpdate = started.HasFirst;
            while (hasUpdate)
            {
                var update = started.Updates.Current;
                foreach (var reported in update.Contents.OfType<UsageContent>())
                {
                    usage.Add(reported.Details);
                }

                yield return update;
                hasUpdate = await started.Updates.MoveNextAsync();
            }
        }
        finally
        {
            await started.Updates.DisposeAsync();
            started.Client.Dispose();

            // In the finally so the spend is settled once whether the stream ended, the consumer stopped early or enumeration threw.
            reservation.Settle(usage);
        }
    }

    /// <inheritdoc/>
    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    /// <inheritdoc/>
    public void Dispose()
    {
    }

    sealed record StartedStream(IChatClient Client, IAsyncEnumerator<ChatResponseUpdate> Updates, bool HasFirst);
}
