// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Primitives;

namespace Cratis.Prologue.Extractor.Shadowing;

/// <summary>
/// Chooses the headers of a request that are copied to the shadow destination.
/// </summary>
/// <remarks>
/// Only headers on an allowlist are copied: the ones that describe the body and what the caller accepts, the trace context, and the
/// User-Agent. Everything else is dropped, including every credential, because the shadow system is not the system the caller signed
/// in to and an unknown header can carry anything. A rule lists the further headers it deliberately forwards in
/// <c language="csharp">forwardHeaders</c>. Hop-by-hop headers, Host and Content-Length describe the original connection and are never copied.
/// <see cref="MarkerHeader"/> tells the shadow system the request is a copy.
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

    static readonly HashSet<string> _allowed = new(StringComparer.OrdinalIgnoreCase)
    {
        "Content-Type", "Content-Encoding", "Accept", "Accept-Language", "Accept-Encoding", "traceparent", "tracestate", "baggage", "User-Agent"
    };

    /// <summary>
    /// Gets whether a header is copied without a rule asking for it.
    /// </summary>
    /// <param name="name">The header name.</param>
    /// <returns>True when it is on the allowlist.</returns>
    public static bool IsAllowed(string name) => _allowed.Contains(name);

    /// <summary>
    /// Chooses the headers to copy.
    /// </summary>
    /// <param name="headers">The headers of the original request.</param>
    /// <param name="forwarded">The further headers the rule forwards on purpose.</param>
    /// <returns>The header names with their values.</returns>
    public static IReadOnlyList<KeyValuePair<string, string[]>> Select(IEnumerable<KeyValuePair<string, StringValues>> headers, ISet<string> forwarded) =>
    [
        .. headers
            .Where(header => !_neverCopied.Contains(header.Key) && !string.Equals(header.Key, MarkerHeader, StringComparison.OrdinalIgnoreCase) && (IsAllowed(header.Key) || forwarded.Contains(header.Key)))
            .Select(header => new KeyValuePair<string, string[]>(header.Key, header.Value.ToArray()!)),
        new KeyValuePair<string, string[]>(MarkerHeader, ["true"])
    ];
}
