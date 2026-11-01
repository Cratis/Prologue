// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Arc.Chronicle.Commands;

namespace Cratis.Prologue.Workbench.Common;

/// <summary>
/// The name of a secret resolved from configuration or the environment when it is needed; the secret's value is never held. It is kept off the causation chain: if somebody types the secret itself where its name belongs, the value must not be copied into every event.
/// </summary>
/// <param name="Value">The underlying value.</param>
[NotAudited]
public record SecretName(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="SecretName"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator SecretName(string value) => new(value);
}
