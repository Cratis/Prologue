// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Contracts;

/// <summary>
/// Represents the schema of a database being watched, captured as evidence when capture starts — the tables with
/// their columns, primary keys, foreign-key relationships, and unique constraints. Only structure is captured,
/// never data values; the downstream interpreter uses it to learn field sizes, required fields, and how the
/// tables relate.
/// </summary>
/// <param name="Engine">The database engine the schema belongs to (for example <c>sqlserver</c> or <c>postgres</c>).</param>
/// <param name="Database">The name of the database the schema belongs to.</param>
/// <param name="Source">The configured logical name of the source the schema was read from.</param>
/// <param name="Tables">The tables that make up the schema.</param>
public record DatabaseSchemaObserved(
    string Engine,
    string Database,
    string Source,
    IReadOnlyList<SchemaTable> Tables) : ObservationPayload;
