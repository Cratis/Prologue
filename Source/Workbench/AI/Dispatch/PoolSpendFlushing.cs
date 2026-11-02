// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Hosting;

namespace Cratis.Prologue.Workbench.AI.Dispatch;

/// <summary>
/// Writes the spend that has waited long enough, and everything that waits when the host stops.
/// </summary>
/// <param name="ledger">The <see cref="IPoolSpendLedger"/>.</param>
public class PoolSpendFlushing(IPoolSpendLedger ledger) : BackgroundService
{
    /// <inheritdoc/>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        await ledger.FlushAll(cancellationToken);
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await ledger.FlushDue(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // The host is stopping; StopAsync writes what is left.
        }
    }
}
