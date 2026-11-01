// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Repositories.Cloning;

/// <summary>
/// Why a clone failed. Credentials are redacted before a reason is made.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record FailureReason(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="FailureReason"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator FailureReason(string value) => new(value);
}
