// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Discover;

/// <summary>
/// What an analyzed system or folder is called.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record SubjectName(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="SubjectName"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator SubjectName(string value) => new(value);
}
