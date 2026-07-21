// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Configuration;
using Cratis.Prologue.Extractor.Capturing;

namespace Cratis.Prologue.Extractor.Sources.Postgres;

/// <summary>
/// Watches a PostgreSQL database via logical replication, publishing one observation per committed transaction
/// that changed at least one table.
/// </summary>
/// <param name="options">The configuration for this PostgreSQL source.</param>
/// <param name="channel">The channel observations are published to.</param>
/// <param name="preparer">Checks the server is configured for logical replication before the reader starts.</param>
/// <param name="logger">The logger.</param>
public class PostgresChangeSource(
    PostgresOptions options,
    IObservationChannel channel,
    PostgresReplicationPreparer preparer,
    ILogger<PostgresChangeSource> logger) : BackgroundService
{
    readonly PostgresReplicationReader _reader = new();

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // wal_level and the role's REPLICATION attribute are server-side settings the extractor cannot fix, and a
        // restart is needed to change wal_level — so once the server answers and says no, retrying every few
        // seconds would only produce noise. Say what is wrong once, clearly, and stop watching this source.
        if (!await WaitUntilReadyForReplication(stoppingToken))
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _reader.EnsureInfrastructure(options, stoppingToken);
                PostgresChangeSourceLog.Watching(logger, options.Name, options.Slot);

                await foreach (var observation in _reader.Stream(options, stoppingToken))
                {
                    await channel.Publish(observation, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                PostgresChangeSourceLog.WatchFailed(logger, options.Name, exception);
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    /// <summary>
    /// Waits for the database to answer the readiness question at all. Not being able to connect yet is the normal
    /// case when the extractor starts alongside the system it captures, and must be retried — while an unhandled
    /// exception here would take the whole extractor down with it, since a faulting background service stops the
    /// host. Only a server that answers and says no is final.
    /// </summary>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/> that stops the wait.</param>
    /// <returns>True when the source can stream changes; false when it must give up.</returns>
    async Task<bool> WaitUntilReadyForReplication(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                return await preparer.IsReadyForReplication(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return false;
            }
            catch (Exception exception)
            {
                PostgresChangeSourceLog.WatchFailed(logger, options.Name, exception);
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        return false;
    }
}
