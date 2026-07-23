// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Configuration;
using Cratis.Prologue.Extractor.Capturing;
using Cratis.Prologue.Extractor.Sources.Schema;
using Microsoft.Data.SqlClient;

namespace Cratis.Prologue.Extractor.Sources.SqlServer;

/// <summary>
/// Captures the schema of a watched SQL Server database as evidence when capture starts — the structural truth
/// the downstream interpreter reads field sizes, required fields, and relationships from. Honors the same table
/// allowlist the change capture uses; only metadata is read, never data values.
/// </summary>
/// <param name="options">The configuration for this SQL Server source.</param>
/// <param name="logger">The logger.</param>
public class SqlServerSchemaCapture(SqlServerOptions options, ILogger<SqlServerSchemaCapture> logger)
{
    readonly SqlServerSchemaReader _reader = new();

    /// <summary>
    /// Reads the database's schema and shapes it into an observation. Failing to read the schema never stops the
    /// source — the change capture is worth having even without the structural evidence.
    /// </summary>
    /// <param name="connection">An open connection to the database being watched.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The schema <see cref="Observation"/>, or <see langword="null"/> when the schema could not be read.</returns>
    public async Task<Observation?> Capture(SqlConnection connection, CancellationToken cancellationToken)
    {
        try
        {
            var rows = await _reader.Read(connection, cancellationToken);
            rows = rows.OnlyTables(SqlServerChangeCapturePreparer.Matching(rows.Tables, [.. options.Tables]));

            var observation = SchemaObservationBuilder.Build(
                SourceKind.SqlServer,
                options.Name,
                connection.Database,
                DateTimeOffset.UtcNow,
                rows);

            SqlServerSchemaCaptureLog.SchemaCaptured(logger, options.Name, rows.Tables.Count, connection.Database);
            return observation;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            SqlServerSchemaCaptureLog.SchemaCaptureFailed(logger, options.Name, exception);
            return null;
        }
    }
}
