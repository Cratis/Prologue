// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Concepts;

namespace Cratis.Prologue.Contracts;

/// <summary>
/// Represents the kind of source an observation originated from (for example <c>http</c>, <c>sqlserver</c>, or <c>postgres</c>).
/// </summary>
/// <param name="Value">The underlying string value.</param>
public record SourceKind(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="SourceKind"/>.
    /// </summary>
    public static readonly SourceKind NotSet = new(string.Empty);

    /// <summary>
    /// Represents the HTTP command source.
    /// </summary>
    public static readonly SourceKind Http = new("http");

    /// <summary>
    /// Represents a SQL Server change source.
    /// </summary>
    public static readonly SourceKind SqlServer = new("sqlserver");

    /// <summary>
    /// Represents a PostgreSQL change source.
    /// </summary>
    public static readonly SourceKind Postgres = new("postgres");

    /// <summary>
    /// Represents an OpenTelemetry (trace) source.
    /// </summary>
    public static readonly SourceKind OpenTelemetry = new("opentelemetry");

    /// <summary>
    /// Implicitly converts a string to a <see cref="SourceKind"/>.
    /// </summary>
    /// <param name="value">The string value to convert.</param>
    public static implicit operator SourceKind(string value) => new(value);
}
