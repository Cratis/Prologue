// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Prologues;

/// <summary>
/// The time window, in milliseconds, within which an HTTP command claims the database transactions and spans it caused.
/// </summary>
/// <param name="Value">The window in milliseconds.</param>
public record CorrelationWindow(int Value) : ConceptAs<int>(Value)
{
    /// <summary>
    /// The window the Extractor uses when nothing else is chosen.
    /// </summary>
    public static readonly CorrelationWindow Default = new(2000);

    /// <summary>
    /// The shortest window that can be chosen.
    /// </summary>
    public const int Minimum = 100;

    /// <summary>
    /// The longest window that can be chosen.
    /// </summary>
    public const int Maximum = 60_000;

    /// <summary>
    /// Implicitly converts an <see cref="int"/> to a <see cref="CorrelationWindow"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator CorrelationWindow(int value) => new(value);
}
