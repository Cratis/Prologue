// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.AI.Network;

/// <summary>
/// Decides whether a provider runs inside the customer's network. This is the same rule the panel applies (networkPosture.ts), so the
/// posture the shell shows and the providers the dispatcher treats as inside agree.
/// </summary>
public static class NetworkPositions
{
    static readonly string[] _privateSuffixes = [".local", ".lan", ".internal", ".intranet", ".home", ".corp"];

    /// <summary>
    /// Decides whether a provider is inside the network. A person's mark wins; with no mark, only an OpenAI-compatible provider with a
    /// private endpoint counts as inside.
    /// </summary>
    /// <param name="isOpenAICompatible">Whether the provider is an OpenAI-compatible one.</param>
    /// <param name="endpoint">The provider's endpoint, when it has one.</param>
    /// <param name="mark">What a person said, or null when nobody did.</param>
    /// <returns>True when the provider is inside the network.</returns>
    public static bool IsInside(bool isOpenAICompatible, string? endpoint, bool? mark) => mark ?? (isOpenAICompatible && IsPrivateEndpoint(endpoint));

    /// <summary>
    /// Decides whether an endpoint points inside a network: localhost, a private or loopback address, a name that ends in a private
    /// suffix, or a bare host name that only a local resolver knows.
    /// </summary>
    /// <param name="endpoint">The endpoint a person typed, with or without a scheme.</param>
    /// <returns>True when the endpoint is private.</returns>
    public static bool IsPrivateEndpoint(string? endpoint)
    {
        var text = endpoint?.Trim();
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        var candidate = text.Contains("://", StringComparison.Ordinal) ? text : $"http://{text}";
        if (!Uri.TryCreate(candidate, UriKind.Absolute, out var uri))
        {
            return false;
        }

        return IsPrivateHost(uri.Host.Trim('[', ']').ToLowerInvariant());
    }

    static bool IsPrivateHost(string host) => IsLoopbackName(host) || IsPrivateAddress(host) || HasPrivateSuffix(host) || IsBareName(host);

    static bool IsLoopbackName(string host) => host == "localhost" || host.EndsWith(".localhost", StringComparison.Ordinal) || host == "::1";

    static bool HasPrivateSuffix(string host) => _privateSuffixes.Any(suffix => host.EndsWith(suffix, StringComparison.Ordinal));

    static bool IsBareName(string host) => !host.Contains('.') && !host.Contains(':');

    static bool IsPrivateAddress(string host)
    {
        var parts = host.Split('.');
        if (parts.Length != 4 || !parts.All(part => int.TryParse(part, out var value) && value is >= 0 and <= 255))
        {
            return false;
        }

        return IsPrivateRange([.. parts.Select(int.Parse)]);
    }

    static bool IsPrivateRange(int[] octets) =>
        octets[0] is 10 or 127
        || (octets[0] == 172 && octets[1] is >= 16 and <= 31)
        || (octets[0] == 192 && octets[1] == 168)
        || (octets[0] == 169 && octets[1] == 254);
}
