// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using Cratis.Prologue.Contracts;
using Cratis.Prologue.Interpreter.Contracts;

namespace Cratis.Prologue.Interpretation;

/// <summary>
/// Infers the validation rules for a command from the constraints the observed schema places on the columns its
/// correlated transactions wrote — a non-nullable column requires a value, and a bounded string column caps its
/// length. Primary-key and identity columns never yield a required rule: the database generates those values, so
/// they are not user input the command must validate.
/// </summary>
public static class ValidationInference
{
    /// <summary>
    /// Infers the validation rules for a command from the columns its correlated transactions wrote.
    /// </summary>
    /// <param name="transactions">The database transactions correlated with the command.</param>
    /// <param name="schema">The observed schema to source constraints from.</param>
    /// <returns>The inferred validation rules.</returns>
    public static IReadOnlyList<ExtractedValidationRule> ForCommand(
        IReadOnlyList<DatabaseTransactionObserved> transactions,
        SchemaIndex schema) =>
    [
        .. transactions
            .SelectMany(transaction => transaction.Tables.Select(table => (transaction.Database, Table: table)))
            .Select(scoped => (scoped.Table, SchemaTable: schema.Find(scoped.Database, scoped.Table.Schema, scoped.Table.Table)))
            .Where(scoped => scoped.SchemaTable is not null)
            .SelectMany(scoped => scoped.Table.Columns.SelectMany(column => ForColumn(scoped.SchemaTable!, column)))
            .GroupBy(rule => (rule.Property, rule.Kind))
            .Select(group => group.First())
    ];

    static IEnumerable<ExtractedValidationRule> ForColumn(SchemaTable table, string column)
    {
        var schemaColumn = table.Column(column);
        if (schemaColumn is null)
        {
            yield break;
        }

        var property = Naming.Pascalize(column);
        if (!schemaColumn.IsNullable && !schemaColumn.IsIdentity && !table.IsPrimaryKey(column))
        {
            yield return new ExtractedValidationRule(
                property,
                ExtractedValidationRuleKind.Required,
                string.Empty,
                $"{property} is required");
        }

        if (SchemaTypeMapping.TypeFor(schemaColumn.DataType) == "string" && schemaColumn.MaxLength > 0)
        {
            yield return new ExtractedValidationRule(
                property,
                ExtractedValidationRuleKind.MaxLength,
                schemaColumn.MaxLength.ToString(CultureInfo.InvariantCulture),
                $"{property} must be at most {schemaColumn.MaxLength} characters");
        }
    }
}
