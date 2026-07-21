// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Configuration;
using Npgsql;

namespace Cratis.Prologue.Extractor.Sources.Postgres;

/// <summary>
/// Checks that the PostgreSQL server can actually be read from before the reader starts trying. The extractor
/// creates its own publication and replication slot, but <c>wal_level</c> is a server setting that only a restart
/// can change — so it is worth saying precisely what is wrong rather than failing on a confusing error from
/// <c>pg_create_logical_replication_slot</c> every few seconds.
/// </summary>
/// <param name="options">The configuration for this PostgreSQL source.</param>
/// <param name="logger">The logger.</param>
public class PostgresReplicationPreparer(PostgresOptions options, ILogger<PostgresReplicationPreparer> logger)
{
    /// <summary>
    /// Determines whether the server is configured for logical replication and the connecting role may use it.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>True when changes can be streamed; false when the server or the role needs changing first.</returns>
    public async Task<bool> IsReadyForReplication(CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(options.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var walLevelCommand = new NpgsqlCommand("SHOW wal_level", connection);
        var walLevel = await Scalar(walLevelCommand, cancellationToken);

        if (!string.Equals(walLevel, "logical", StringComparison.Ordinal))
        {
            PostgresReplicationPreparerLog.WalLevelNotLogical(logger, options.Name, walLevel);
            return false;
        }

        await using var roleCommand = new NpgsqlCommand(
            "SELECT (rolreplication OR rolsuper)::text FROM pg_roles WHERE rolname = current_user",
            connection);
        var mayReplicate = await Scalar(roleCommand, cancellationToken);

        if (!string.Equals(mayReplicate, "True", StringComparison.OrdinalIgnoreCase))
        {
            PostgresReplicationPreparerLog.RoleCannotReplicate(logger, options.Name);
            return false;
        }

        return true;
    }

    static async Task<string> Scalar(NpgsqlCommand command, CancellationToken cancellationToken)
    {
        var result = await command.ExecuteScalarAsync(cancellationToken);

        return result?.ToString() ?? string.Empty;
    }
}
