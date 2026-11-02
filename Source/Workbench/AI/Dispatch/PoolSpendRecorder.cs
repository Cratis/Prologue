// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.AI.Providers.Pools;
using Cratis.Prologue.Workbench.AI.Pools.Spending;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace Cratis.Prologue.Workbench.AI.Dispatch;

/// <summary>
/// Records a pool's spend through the command pipeline. The tokens are the ones the provider reported on the answer.
/// </summary>
/// <param name="pipeline">Executes the command.</param>
/// <param name="time">Tells the day.</param>
/// <param name="logger">The logger.</param>
public class PoolSpendRecorder(ICommandPipeline pipeline, TimeProvider time, ILogger<PoolSpendRecorder> logger) : IPoolSpendRecorder
{
    /// <summary>
    /// Counts the tokens a usage reported: its total, or its input and output when there is no total.
    /// </summary>
    /// <param name="usage">The usage, if reported.</param>
    /// <returns>The tokens; zero when nothing was reported.</returns>
    public static long TokensOf(UsageDetails? usage) => usage is null ? 0 : usage.TotalTokenCount ?? ((usage.InputTokenCount ?? 0) + (usage.OutputTokenCount ?? 0));

    /// <inheritdoc/>
    public async Task Record(string poolKey, UsageDetails? usage, CancellationToken cancellationToken)
    {
        var tokens = TokensOf(usage);
        if (tokens <= 0)
        {
            return;
        }

        try
        {
            var result = await pipeline.Execute(new RecordPoolSpend(new AIProviderPoolId(Guid.Parse(poolKey)), PoolPlans.DayOf(time), tokens), cancellationToken);
            if (!result.IsSuccess)
            {
                logger.SpendNotRecorded(poolKey);
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.SpendNotRecorded(poolKey, exception);
        }
    }
}
