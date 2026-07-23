// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Interpretation;

/// <summary>
/// Maps observed database column types to the property types the extracted model uses — the schema evidence that
/// takes precedence over name-convention inference. Both the SQL Server catalog names and the PostgreSQL
/// information-schema spellings are recognized; an unknown type maps to nothing so the caller falls back to the
/// name conventions.
/// </summary>
public static class SchemaTypeMapping
{
    /// <summary>
    /// Maps an engine-specific column data type to the property type it evidences.
    /// </summary>
    /// <param name="dataType">The engine-specific data type name (for example <c>nvarchar</c> or <c>uuid</c>).</param>
    /// <returns>The property type, or <see langword="null"/> when the data type is not recognized.</returns>
    public static string? TypeFor(string dataType) =>
        dataType.ToLowerInvariant() switch
        {
            "varchar" or "nvarchar" or "text" or "character varying" => "string",
            "uniqueidentifier" or "uuid" => "Guid",
            "int" or "integer" => "int",
            "bigint" => "long",
            "bit" or "boolean" => "bool",
            "datetime" or "datetime2" or "timestamp" or "timestamptz"
                or "timestamp with time zone" or "timestamp without time zone" => "DateTimeOffset",
            "date" => "DateOnly",
            "decimal" or "numeric" or "money" => "decimal",
            _ => null
        };
}
