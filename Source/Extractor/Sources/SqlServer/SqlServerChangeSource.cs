// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Configuration;
using Cratis.Prologue.Extractor.Capturing;
using Microsoft.Data.SqlClient;

namespace Cratis.Prologue.Extractor.Sources.SqlServer;

/// <summary>
/// Watches a CDC-enabled SQL Server database by polling its change tables, grouping changes per transaction, and
/// publishing the table/column metadata as observations. When capture starts, the database's schema is published
/// once as structural evidence.
/// </summary>
/// <param name="options">The configuration for this SQL Server source.</param>
/// <param name="channel">The channel observations are published to.</param>
/// <param name="preparer">Turns Change Data Capture on so the system being captured does not have to.</param>
/// <param name="schemaCapture">Captures the database's schema as evidence when capture starts.</param>
/// <param name="logger">The logger.</param>
public class SqlServerChangeSource(
    SqlServerOptions options,
    IObservationChannel channel,
    SqlServerChangeCapturePreparer preparer,
    SqlServerSchemaCapture schemaCapture,
    ILogger<SqlServerChangeSource> logger) : BackgroundService
{
    readonly SqlServerCdcReader _reader = new();
    bool _schemaCaptured;

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

        await preparer.Prepare(connection, stoppingToken);
        await CaptureSchemaOnce(connection, stoppingToken);

        var instances = await _reader.LoadInstances(connection, stoppingToken);

        if (instances.Count == 0)
        {
            // Nothing to watch means nothing will ever be captured, and a silent idle loop is the worst way to
            // find that out.
            SqlServerChangeSourceLog.NothingToWatch(logger, options.Name, connection.Database);
        }
        else
        {
            SqlServerChangeSourceLog.Watching(logger, options.Name, instances.Count, connection.Database);
        }

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

    /// <summary>
    /// Publishes the database's schema as an observation the first time it can be read. Watch is re-entered on
    /// failure, so the guard keeps a reconnect from repeating evidence that was already captured.
    /// </summary>
    /// <param name="connection">An open connection to the database being watched.</param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    async Task CaptureSchemaOnce(SqlConnection connection, CancellationToken stoppingToken)
    {
        if (_schemaCaptured)
        {
            return;
        }

        var observation = await schemaCapture.Capture(connection, stoppingToken);
        if (observation is not null)
        {
            await channel.Publish(observation, stoppingToken);
            _schemaCaptured = true;
        }
    }
}
