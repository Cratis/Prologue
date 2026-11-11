// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Configuration;
using Cratis.Prologue.Workbench.Common;

namespace Cratis.Prologue.Workbench.Repositories.Connecting;

/// <summary>
/// The rules every repository connection shares.
/// </summary>
public static class ConnectionRules
{
    static readonly string[] _tokenPrefixes = ["ghp_", "gho_", "ghs_", "ghu_", "github_pat_", "glpat-", "xoxb-", "sk-"];

    /// <summary>
    /// Gets why a connection to a host cannot be recorded, when it cannot.
    /// </summary>
    /// <param name="url">The server or organization address.</param>
    /// <param name="token">The name of the secret holding the access token.</param>
    /// <returns>The refusal, or null when the connection can be recorded.</returns>
    public static ValidationResult? Refuse(ServerUrl url, SecretName token) =>
        RefuseUrl(url) ?? RefuseSecretName(token);

    /// <summary>
    /// Gets why a server address cannot be used, when it cannot.
    /// </summary>
    /// <param name="url">The address.</param>
    /// <returns>The refusal, or null when the address can be used.</returns>
    public static ValidationResult? RefuseUrl(ServerUrl url) =>
        url.IsValid ? null : ValidationResult.Error("Give the address of the server, starting with https://");

    /// <summary>
    /// Gets why a GitHub App connection cannot be recorded, when it cannot. The app signs requests with its private key, so the API
    /// has to be reached over https and be the API of the server the repositories are cloned from.
    /// </summary>
    /// <param name="url">The GitHub server.</param>
    /// <param name="api">The address of the GitHub API.</param>
    /// <param name="app">The app.</param>
    /// <param name="installation">The installation.</param>
    /// <returns>The refusal, or null when the connection can be recorded.</returns>
    public static ValidationResult? RefuseApp(ServerUrl url, ServerUrl api, AppId app, InstallationId installation)
    {
        if (app.Value <= 0 || installation.Value <= 0)
        {
            return ValidationResult.Error("Give the id of the GitHub App and of its installation");
        }

        if (!api.IsValid || !Uri.TryCreate(api.Value, UriKind.Absolute, out var apiUri) || apiUri.Scheme != Uri.UriSchemeHttps)
        {
            return ValidationResult.Error("The GitHub API address has to start with https://");
        }

        return Uri.TryCreate(url.Value, UriKind.Absolute, out var server) && IsOwnHost(apiUri, server)
            ? null
            : ValidationResult.Error("The GitHub API has to be on the server's own host, such as https://api.github.com for github.com or https://<host>/api/v3 for GitHub Enterprise");
    }

    /// <summary>
    /// Gets why a secret name cannot be recorded, when it cannot. Only the name of a secret is ever recorded, so a value
    /// that looks like a credential is refused rather than written into the event log.
    /// </summary>
    /// <param name="name">The secret's name.</param>
    /// <returns>The refusal, or null when it can be recorded.</returns>
    public static ValidationResult? RefuseSecretName(SecretName name)
    {
        var value = name.Value.Trim();
        if (value.Length == 0)
        {
            return ValidationResult.Error("Name the secret that holds the access token or private key");
        }

        if (LooksLikeCredential(value))
        {
            return ValidationResult.Error("That looks like the token itself. Enter the name of the secret that holds it; the token is never stored.");
        }

        return SecretPlaceholders.IsValidName(value)
            ? null
            : ValidationResult.Error("A secret name is 1 to 64 letters, digits, underscores, dots, slashes or hyphens.");
    }

    /// <summary>
    /// Gets whether a value looks like a credential rather than the name of one.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>True when it does.</returns>
    public static bool LooksLikeCredential(string value) => value.Any(char.IsWhiteSpace) || HasTokenPrefix(value) || IsLongToken(value);

    static bool IsOwnHost(Uri api, Uri server) =>
        string.Equals(api.Host, server.Host, StringComparison.OrdinalIgnoreCase) || string.Equals(api.Host, $"api.{server.Host}", StringComparison.OrdinalIgnoreCase);

    static bool HasTokenPrefix(string value) => _tokenPrefixes.Any(prefix => value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

    static bool IsLongToken(string value) => value.Length >= 40 && value.All(char.IsLetterOrDigit);
}
