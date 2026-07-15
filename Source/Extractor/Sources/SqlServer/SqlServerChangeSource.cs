// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Configuration;
using Cratis.Prologue.Extractor.Capturing;
using Microsoft.Data.SqlClient;

namespace Cratis.Prologue.Extractor.Sources.SqlServer;

/// <summary>
/// Watches a CDC-enabled SQL Server database by polling its change tables, grouping changes per transaction, and
/// publishing the table/column metadata as observations.
/// </summary>
/// <param name="options">The configuration for this SQL Server source.</param>
/// <param name="channel">The channel observations are published to.</param>
/// <param name="logger">The logger.</param>
public class SqlServerChangeSource(
    SqlServerOptions options,
    IObservationChannel channel,
    ILogger<SqlServerChangeSource> logger) : BackgroundService
{
    readonly SqlServerCdcReader _reader = new();

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Watch(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                SqlServerChangeSourceLog.WatchFailed(logger, options.Name, exception);
                await Task.Delay(options.PollInterval, stoppingToken);
            }
        }
    }

    static int Compare(byte[] left, byte[] right)
    {
        for (var index = 0; index < Math.Min(left.Length, right.Length); index++)
        {
            var comparison = left[index].CompareTo(right[index]);
            if (comparison != 0)
            {
                return comparison;
            }
        }

        return left.Length.CompareTo(right.Length);
    }

    async Task Watch(CancellationToken stoppingToken)
    {
        await using var connection = new SqlConnection(options.ConnectionString);
        await connection.OpenAsync(stoppingToken);

        var instances = await _reader.LoadInstances(connection, stoppingToken);
        SqlServerChangeSourceLog.Watching(logger, options.Name, instances.Count, connection.Database);

        var lastLsn = await _reader.GetMaxLsn(connection, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(options.PollInterval, stoppingToken);

            var maxLsn = await _reader.GetMaxLsn(connection, stoppingToken);
            if (Compare(maxLsn, lastLsn) <= 0)
            {
                continue;
            }

            var rows = await _reader.ReadChanges(connection, instances, lastLsn, maxLsn, stoppingToken);
            foreach (var observation in CdcChangeDecoder.GroupIntoTransactions(rows, SourceKind.SqlServer, connection.Database))
            {
                await channel.Publish(observation, stoppingToken);
            }

            lastLsn = maxLsn;
        }
    }
}
