// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Primitives;

namespace Cratis.Prologue.Extractor.Shadowing;

/// <summary>
/// Chooses the headers of a request that are copied to the shadow destination.
/// </summary>
/// <remarks>
/// Hop-by-hop headers belong to the connection between the caller and the proxy, and Host and Content-Length describe the original
/// request, so none of them are copied. Headers that carry credentials (Authorization, Cookie, and any header whose name says it holds
/// a key, token, secret or password) are dropped too: the shadow system is not the system the caller signed in to. A rule lists the
/// ones it deliberately forwards. Everything else is copied, and <see cref="MarkerHeader"/> tells the shadow system the request is a copy.
/// </remarks>
public static class ShadowHeaders
{
    /// <summary>
    /// The header added to every mirrored request so the shadow system can tell it is a copy.
    /// </summary>
    public const string MarkerHeader = "X-Prologue-Shadow";

    static readonly HashSet<string> _neverCopied = new(StringComparer.OrdinalIgnoreCase)
    {
        "Connection", "Keep-Alive", "Proxy-Authenticate", "Proxy-Authorization", "Proxy-Connection", "TE", "Trailer",
        "Transfer-Encoding", "Upgrade", "Host", "Content-Length", "Expect"
    };

    static readonly string[] _credentialNames = ["authorization", "cookie", "key", "token", "secret", "password", "credential"];

    /// <summary>
    /// Gets whether a header carries credentials by its name.
    /// </summary>
    /// <param name="name">The header name.</param>
    /// <returns>True when it does.</returns>
    public static bool CarriesCredentials(string name) =>
        _credentialNames.Any(part => name.Contains(part, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Chooses the headers to copy.
    /// </summary>
    /// <param name="headers">The headers of the original request.</param>
    /// <param name="forwarded">The credential-bearing headers the rule forwards on purpose.</param>
    /// <returns>The header names with their values.</returns>
    public static IReadOnlyList<KeyValuePair<string, string[]>> Select(IEnumerable<KeyValuePair<string, StringValues>> headers, ISet<string> forwarded) =>
    [
        .. headers
            .Where(header => !_neverCopied.Contains(header.Key) && (!CarriesCredentials(header.Key) || forwarded.Contains(header.Key)))
            .Select(header => new KeyValuePair<string, string[]>(header.Key, header.Value.ToArray()!)),
        new KeyValuePair<string, string[]>(MarkerHeader, ["true"])
    ];
}
