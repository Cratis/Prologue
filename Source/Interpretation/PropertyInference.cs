// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Contracts;
using Cratis.Prologue.Interpreter.Contracts;

namespace Cratis.Prologue.Interpretation;

/// <summary>
/// Infers the properties of commands, events, and read models from the columns observed changing in database
/// transactions and the allowlisted attribute keys observed on telemetry spans. No data values are ever seen — only
/// column and attribute names — so types are inferred purely from naming conventions.
/// </summary>
public static class PropertyInference
{
    /// <summary>
    /// Infers the input properties of a command from the columns its correlated transactions changed and the
    /// attribute keys its spans carried.
    /// </summary>
    /// <param name="transactions">The database transactions correlated with the command.</param>
    /// <param name="spans">The telemetry spans correlated with the command.</param>
    /// <returns>The inferred command properties.</returns>
    public static IReadOnlyList<ExtractedProperty> ForCommand(
        IReadOnlyList<DatabaseTransactionObserved> transactions,
        IReadOnlyList<TelemetryObserved> spans)
    {
        var columns = transactions.SelectMany(transaction => transaction.Tables).SelectMany(table => table.Columns);
        var attributes = spans.SelectMany(span => span.AttributeKeys);
        return Distinct(columns.Concat(attributes));
    }

    /// <summary>
    /// Infers the properties of a read model for a specific entity from the columns changed on the tables that map
    /// to that entity.
    /// </summary>
    /// <param name="transactions">The database transactions the entity's read model is built from.</param>
    /// <param name="entity">The singular, PascalCase entity name.</param>
    /// <returns>The inferred read-model properties.</returns>
    public static IReadOnlyList<ExtractedProperty> ForEntity(
        IReadOnlyList<DatabaseTransactionObserved> transactions,
        string entity)
    {
        var columns = transactions
            .SelectMany(transaction => transaction.Tables)
            .Where(table => Naming.Singularize(Naming.Pascalize(table.Table)) == entity)
            .SelectMany(table => table.Columns);
        return Distinct(columns);
    }

    static IReadOnlyList<ExtractedProperty> Distinct(IEnumerable<string> names) =>
    [
        .. names
            .Select(Naming.Pascalize)
            .Distinct()
            .Select(name => new ExtractedProperty(name, Naming.InferType(name)))
    ];
}
