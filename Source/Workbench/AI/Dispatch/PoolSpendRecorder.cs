// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.AI.Providers.Pools;
using Cratis.Prologue.Workbench.AI.Pools.Spending;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cratis.Prologue.Workbench.AI.Dispatch;

/// <summary>
/// Records a pool's spend through the command pipeline, in its own scope so the singleton ledger can call it from any thread.
/// </summary>
/// <param name="scopes">Creates the scope the command runs in.</param>
/// <param name="logger">The logger.</param>
public class PoolSpendRecorder(IServiceScopeFactory scopes, ILogger<PoolSpendRecorder> logger) : IPoolSpendRecorder
{
    /// <summary>
    /// Counts the tokens a usage reported: its total, or its input and output when there is no total.
    /// </summary>
    /// <param name="usage">The usage, if reported.</param>
    /// <returns>The tokens; zero when nothing was reported.</returns>
    public static long TokensOf(UsageDetails? usage) => usage is null ? 0 : usage.TotalTokenCount ?? ((usage.InputTokenCount ?? 0) + (usage.OutputTokenCount ?? 0));

    /// <inheritdoc/>
    public async Task<bool> Record(string poolKey, string day, long tokens, CancellationToken cancellationToken)
    {
        if (tokens <= 0)
        {
            return true;
        }

        try
        {
            using var scope = scopes.CreateScope();
            var pipeline = scope.ServiceProvider.GetRequiredService<ICommandPipeline>();
            var result = await pipeline.Execute(new RecordPoolSpend(new AIProviderPoolId(Guid.Parse(poolKey)), day, tokens), cancellationToken);
            if (!result.IsSuccess)
            {
                logger.SpendNotRecorded(poolKey);
            }

            return result.IsSuccess;
        }
        catch (Exception exception) when (!(exception is OperationCanceledException && cancellationToken.IsCancellationRequested))
        {
            logger.SpendNotRecorded(poolKey, exception);
            return false;
        }
    }
}
