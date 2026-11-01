// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Captures;

namespace Cratis.Prologue.Workbench.Rebuild.Drift;

/// <summary>
/// Identifies one piece of drift: a command shape the model does not cover, or a column a known shape started writing.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record DriftKey(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="DriftKey"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator DriftKey(string value) => new(value);

    /// <summary>
    /// Gets the key of a command shape the model does not cover.
    /// </summary>
    /// <param name="shape">The shape.</param>
    /// <returns>The key.</returns>
    public static DriftKey ForShape(CommandShape shape) => new($"shape|{shape.Value}");

    /// <summary>
    /// Gets the key of a column a known shape started writing.
    /// </summary>
    /// <param name="shape">The shape.</param>
    /// <param name="column">The column.</param>
    /// <returns>The key.</returns>
    public static DriftKey ForColumn(CommandShape shape, WrittenColumn column) => new($"column|{shape.Value}|{column.Value}");
}
