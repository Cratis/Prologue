// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Extractor.Sources.Postgres;

internal static partial class PostgresReplicationPreparerLog
{
    [LoggerMessage(LogLevel.Error, "PostgreSQL source '{Source}' cannot stream changes: wal_level is '{WalLevel}', not 'logical'. Start the server with '-c wal_level=logical' (this needs a restart); no database changes will be captured until then")]
    internal static partial void WalLevelNotLogical(ILogger logger, string source, string walLevel);

    [LoggerMessage(LogLevel.Error, "PostgreSQL source '{Source}' cannot stream changes: the connecting role has neither the REPLICATION attribute nor superuser. Grant it with 'ALTER ROLE <role> REPLICATION'; no database changes will be captured until then")]
    internal static partial void RoleCannotReplicate(ILogger logger, string source);
}
