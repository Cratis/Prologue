// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Configuration;
using Cratis.Prologue.Extractor.Capturing;
using Cratis.Prologue.Extractor.Sources.Schema;
using Npgsql;

namespace Cratis.Prologue.Extractor.Sources.Postgres;

/// <summary>
/// Captures the schema of a watched PostgreSQL database as evidence when capture starts — the structural truth
/// the downstream interpreter reads field sizes, required fields, and relationships from. Only metadata is read,
/// never data values.
/// </summary>
/// <param name="options">The configuration for this PostgreSQL source.</param>
/// <param name="logger">The logger.</param>
public class PostgresSchemaCapture(PostgresOptions options, ILogger<PostgresSchemaCapture> logger)
{
    readonly PostgresSchemaReader _reader = new();

    /// <summary>
    /// Reads the database's schema and shapes it into an observation. Failing to read the schema never stops the
    /// source — the change capture is worth having even without the structural evidence.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The schema <see cref="Observation"/>, or <see langword="null"/> when the schema could not be read.</returns>
    public async Task<Observation?> Capture(CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = new NpgsqlConnection(options.ConnectionString);
            await connection.OpenAsync(cancellationToken);

            var rows = await _reader.Read(connection, cancellationToken);
            var observation = SchemaObservationBuilder.Build(
                SourceKind.Postgres,
                options.Name,
                connection.Database,
                DateTimeOffset.UtcNow,
                rows);

            PostgresSchemaCaptureLog.SchemaCaptured(logger, options.Name, rows.Tables.Count, connection.Database);
            return observation;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            PostgresSchemaCaptureLog.SchemaCaptureFailed(logger, options.Name, exception);
            return null;
        }
    }
}
