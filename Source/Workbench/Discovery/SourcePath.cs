// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Discovery;

/// <summary>
/// The path of a source file, relative to the folder that was analyzed.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record SourcePath(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="SourcePath"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator SourcePath(string value) => new(value);
}
