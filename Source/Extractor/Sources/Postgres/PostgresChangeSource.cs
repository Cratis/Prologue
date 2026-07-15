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
/// <param name="logger">The logger.</param>
public class PostgresChangeSource(
    PostgresOptions options,
    IObservationChannel channel,
    ILogger<PostgresChangeSource> logger) : BackgroundService
{
    readonly PostgresReplicationReader _reader = new();

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
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
}
