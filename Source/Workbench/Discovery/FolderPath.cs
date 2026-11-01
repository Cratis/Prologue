// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Discovery;

/// <summary>
/// A folder on the machine the Receiver runs on.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record FolderPath(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="FolderPath"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator FolderPath(string value) => new(value);
}
