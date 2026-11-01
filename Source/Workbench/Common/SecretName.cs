// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Common;

/// <summary>
/// The name of a secret resolved from configuration or the environment when it is needed; the secret's value is never held.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record SecretName(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="SecretName"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator SecretName(string value) => new(value);
}
