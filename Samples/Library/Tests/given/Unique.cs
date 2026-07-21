// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;

namespace Library.Tests.given;

/// <summary>
/// Values no other spec will produce. The distributed application is started once and shared, so every spec adds to
/// the same library — a name or an ISBN has to be unique for an assertion about "the row this spec created" to mean
/// anything.
/// </summary>
public static class Unique
{
    /// <summary>
    /// Produces a name that reads like the thing it stands for, with enough noise on the end to be unique.
    /// </summary>
    /// <param name="stem">The recognizable part of the name.</param>
    /// <returns>The unique name.</returns>
    public static string Name(string stem) => $"{stem}{Suffix()}";

    /// <summary>
    /// Produces a unique 13-digit ISBN.
    /// </summary>
    /// <returns>The unique ISBN.</returns>
    public static string Isbn() => $"978{Random.Shared.NextInt64(1_000_000_000, 9_999_999_999).ToString(CultureInfo.InvariantCulture)}";

    static string Suffix() => Guid.NewGuid().ToString("N")[..6];
}
