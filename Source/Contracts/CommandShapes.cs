// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Contracts;

/// <summary>
/// Works out the shape of an HTTP command: its method and its path with the identities in it generalized, such as
/// "POST /api/loans/{id}/return". The Extractor and the Workbench both use it, so a shape means the same on both sides.
/// </summary>
public static class CommandShapes
{
    /// <summary>
    /// The text an identity in a path is replaced by.
    /// </summary>
    public const string IdentitySegment = "{id}";

    /// <summary>
    /// Works out the shape of a command. The method is upper-cased, the query string and a trailing slash are removed,
    /// and every path segment that is numeric, a GUID, or at least four characters containing a digit (like an ISBN)
    /// becomes "{id}".
    /// </summary>
    /// <param name="method">The HTTP method.</param>
    /// <param name="path">The request path, with or without a query string.</param>
    /// <returns>The shape.</returns>
    public static string From(string method, string path)
    {
        var segments = SegmentsOf(path).Select(segment => IsIdentifier(segment) ? IdentitySegment : segment);
        return $"{method.Trim().ToUpperInvariant()} /{string.Join('/', segments)}";
    }

    /// <summary>
    /// Gets the identities in a path: the segments that <see cref="From"/> replaces.
    /// </summary>
    /// <param name="path">The request path, with or without a query string.</param>
    /// <returns>The identity segments, in the order they appear.</returns>
    public static IEnumerable<string> IdentifiersIn(string path) => SegmentsOf(path).Where(IsIdentifier);

    /// <summary>
    /// Gets whether a path segment is an identity.
    /// </summary>
    /// <param name="segment">The segment.</param>
    /// <returns>True when it is numeric, a GUID, or at least four characters containing a digit.</returns>
    public static bool IsIdentifier(string segment) =>
        segment.All(char.IsAsciiDigit) ||
        Guid.TryParse(segment, out _) ||
        (segment.Length >= 4 && segment.Any(char.IsAsciiDigit));

    static string[] SegmentsOf(string path) =>
        path.Split('?', '#')[0].Trim().Split('/', StringSplitOptions.RemoveEmptyEntries);
}
