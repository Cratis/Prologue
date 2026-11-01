// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Discovery;

/// <summary>
/// What an analysis read: the commit of a clone, or the folder it was pointed at.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record AnalysisSource(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="AnalysisSource"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator AnalysisSource(string value) => new(value);
}
