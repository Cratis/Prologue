// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Screenplay;

/// <summary>
/// Maps the property types the extracted model uses (the C# type names the interpreter infers from schema evidence
/// and naming conventions) to the built-in Screenplay type names. An unrecognized type maps to <c>String</c> — the
/// safest declaration for a developer to refine.
/// </summary>
public static class ScreenplayTypeMapping
{
    /// <summary>
    /// Maps an extracted property type to the Screenplay type name it declares as.
    /// </summary>
    /// <param name="type">The extracted property type (for example <c>Guid</c> or <see langword="string"/>).</param>
    /// <returns>The Screenplay type name.</returns>
    public static string TypeFor(string type) =>
        type switch
        {
            "Guid" => "Uuid",
            "int" or "long" => "Int",
            "decimal" => "Decimal",
            "bool" => "Bool",
            "DateOnly" => "Date",
            "DateTimeOffset" => "DateTime",
            _ => "String"
        };
}
