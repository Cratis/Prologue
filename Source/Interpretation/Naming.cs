// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Contracts;

namespace Cratis.Prologue.Interpretation;

/// <summary>
/// Deterministic naming and type-inference helpers the heuristic model builder uses to turn observed paths, tables,
/// columns, and operations into provisional module, feature, slice, command, event, and property names. These names
/// are intentionally mechanical — the LLM refinement pass improves them into domain language afterwards.
/// </summary>
public static class Naming
{
    /// <summary>
    /// Turns a raw token (a path segment, table, or column) into a PascalCase identifier.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The PascalCase form, or <c>Unknown</c> when the value has no letters or digits.</returns>
    public static string Pascalize(string value)
    {
        var words = value.Split(['_', '-', ' ', '.', '/'], StringSplitOptions.RemoveEmptyEntries);
        var pascalized = string.Concat(words.Select(Capitalize));
        return pascalized.Length == 0 ? "Unknown" : pascalized;
    }

    /// <summary>
    /// Turns a raw token (a path segment, table, or column) into a camelCase identifier — the casing used when a
    /// constraint references a property.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The camelCase form.</returns>
    public static string Camelize(string value)
    {
        var pascalized = Pascalize(value);
        return char.ToLowerInvariant(pascalized[0]) + pascalized[1..];
    }

    /// <summary>
    /// Produces a naive singular form of a plural token (for turning table names into entity names).
    /// </summary>
    /// <param name="value">The value to singularize.</param>
    /// <returns>The singularized value.</returns>
    public static string Singularize(string value) =>
        value switch
        {
            _ when value.EndsWith("ies", StringComparison.OrdinalIgnoreCase) && value.Length > 3 => $"{value[..^3]}y",
            _ when value.EndsWith("ses", StringComparison.OrdinalIgnoreCase) && value.Length > 3 => value[..^2],
            _ when value.EndsWith('s') && !value.EndsWith("ss", StringComparison.OrdinalIgnoreCase) && value.Length > 1 => value[..^1],
            _ => value
        };

    /// <summary>
    /// Builds a past-tense event name from an entity name and the database operation applied to it.
    /// </summary>
    /// <param name="entity">The singular, PascalCase entity name.</param>
    /// <param name="operation">The change operation observed.</param>
    /// <returns>A provisional past-tense event name (for example <c>AuthorCreated</c>).</returns>
    public static string EventName(string entity, ChangeOperation operation) =>
        $"{entity}{operation switch
        {
            ChangeOperation.Insert => "Created",
            ChangeOperation.Update => "Updated",
            ChangeOperation.Delete => "Deleted",
            _ => "Changed"
        }}";

    /// <summary>
    /// Builds an imperative command name from an HTTP method and the resource it acted on.
    /// </summary>
    /// <param name="method">The HTTP method observed.</param>
    /// <param name="entity">The singular, PascalCase resource name.</param>
    /// <returns>A provisional imperative command name (for example <c>CreateAuthor</c>).</returns>
    public static string CommandName(string method, string entity) =>
        $"{method.ToUpperInvariant() switch
        {
            "POST" => "Create",
            "PUT" => "Update",
            "PATCH" => "Update",
            "DELETE" => "Delete",
            _ => "Handle"
        }}{entity}";

    /// <summary>
    /// Infers a C# type name for a column or attribute from naming conventions.
    /// </summary>
    /// <param name="name">The column or attribute name.</param>
    /// <returns>The inferred type name (for example <see cref="Guid"/>, <see langword="int"/>, <see cref="DateTimeOffset"/>, or <see langword="string"/>).</returns>
    public static string InferType(string name)
    {
        var lowered = name.ToLowerInvariant();
        return lowered switch
        {
            _ when lowered.EndsWith("id") => "Guid",
            _ when lowered.EndsWith("count") || lowered.EndsWith("number") || lowered.EndsWith("quantity") => "int",
            _ when lowered.EndsWith("amount") || lowered.EndsWith("price") || lowered.EndsWith("total") => "decimal",
            _ when lowered.EndsWith("at") || lowered.EndsWith("date") || lowered.EndsWith("on") => "DateTimeOffset",
            _ when lowered.StartsWith("is") || lowered.StartsWith("has") => "bool",
            _ => "string"
        };
    }

    // Uppercase the first character and preserve the rest, so an already-PascalCase token (AuthorId) survives and a
    // lowercase snake-case part (author) becomes Author without flattening any interior casing.
    static string Capitalize(string word) =>
        word.Length switch
        {
            0 => string.Empty,
            1 => word.ToUpperInvariant(),
            _ => char.ToUpperInvariant(word[0]) + word[1..]
        };
}
