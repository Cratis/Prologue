// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Captures;

/// <summary>
/// The identity of a capture, the one the Extractor gave it.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record CaptureId(Guid Value) : ConceptAs<Guid>(Value)
{
    /// <summary>
    /// Implicitly converts a <see cref="Guid"/> to a <see cref="CaptureId"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator CaptureId(Guid value) => new(value);
}
