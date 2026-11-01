// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Captures;

/// <summary>
/// The shape of an HTTP command: its method and its path with the identities in it generalized, such as
/// "POST /api/loans/{id}/return". Captures of the same shape are the same behavior seen again.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record CommandShape(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Gets the shape of a capture that had no HTTP command.
    /// </summary>
    public static readonly CommandShape None = new(string.Empty);

    /// <summary>
    /// Gets a value indicating whether this is <see cref="None"/>.
    /// </summary>
    public bool IsNone => string.IsNullOrEmpty(Value);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="CommandShape"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator CommandShape(string value) => new(value);

    /// <summary>
    /// Works out the shape of a command. The method is upper-cased, the query string and a trailing slash are removed,
    /// and every path segment that is numeric, a GUID, or at least four characters containing a digit (like an ISBN)
    /// becomes "{id}".
    /// </summary>
    /// <param name="method">The HTTP method.</param>
    /// <param name="path">The request path, with or without a query string.</param>
    /// <returns>The shape.</returns>
    public static CommandShape From(string method, string path) => new(CommandShapes.From(method, path));

    /// <summary>
    /// Gets the identities in a path, such as the loan number in "/api/loans/4412/return".
    /// </summary>
    /// <param name="path">The request path.</param>
    /// <returns>The identity segments.</returns>
    public static IEnumerable<string> IdentifiersIn(string path) => CommandShapes.IdentifiersIn(path);
}
