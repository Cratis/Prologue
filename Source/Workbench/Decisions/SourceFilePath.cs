// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Decisions;

/// <summary>
/// The path of a source file a decision point was found in.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record SourceFilePath(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Gets the value representing none.
    /// </summary>
    public static readonly SourceFilePath None = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="SourceFilePath"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator SourceFilePath(string value) => new(value);
}
