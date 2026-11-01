// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.RegularExpressions;

namespace Cratis.Prologue.Workbench.Repositories.Cloning;

/// <summary>
/// Removes credentials from text before it is kept or shown.
/// </summary>
public static partial class SecretRedaction
{
    /// <summary>
    /// The text a credential is replaced with.
    /// </summary>
    public const string Mask = "***";

    [GeneratedRegex(@"(?<scheme>://)[^/@\s]+@", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex UserInfo { get; }

    [GeneratedRegex(@"(?<header>authorization:\s*(?:basic|bearer)\s+)\S+", RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 1000)]
    private static partial Regex AuthorizationHeader { get; }

    /// <summary>
    /// Redacts known secret values and any credentials in addresses from a text.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="secrets">The secret values known to be in play.</param>
    /// <returns>The text with every credential replaced by <see cref="Mask"/>.</returns>
    public static string Redact(string text, IEnumerable<string> secrets)
    {
        var redacted = text;
        foreach (var secret in secrets.Where(secret => secret.Length > 0).OrderByDescending(secret => secret.Length))
        {
            redacted = redacted.Replace(secret, Mask, StringComparison.Ordinal);
        }

        redacted = UserInfo.Replace(redacted, "${scheme}" + Mask + "@");
        return AuthorizationHeader.Replace(redacted, "${header}" + Mask);
    }
}
