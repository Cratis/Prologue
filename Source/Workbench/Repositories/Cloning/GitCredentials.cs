// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using System.Text;
using Cratis.Prologue.Workbench.Systems;

namespace Cratis.Prologue.Workbench.Repositories.Cloning;

/// <summary>
/// Hands an access token to git. It goes into the environment of the git process as an authorization header, so it is never
/// part of an address, a command line or anything that is returned.
/// </summary>
public static class GitCredentials
{
    /// <summary>
    /// Gets the environment for a git process.
    /// </summary>
    /// <param name="host">The kind of host.</param>
    /// <param name="token">The access token; null for an anonymous clone.</param>
    /// <returns>The environment variables.</returns>
    public static IReadOnlyDictionary<string, string> Environment(RepositoryHost host, string? token)
    {
        if (token is null)
        {
            return ReadOnlyDictionary<string, string>.Empty;
        }

        return new Dictionary<string, string>
        {
            ["GIT_CONFIG_COUNT"] = "1",
            ["GIT_CONFIG_KEY_0"] = "http.extraHeader",
            ["GIT_CONFIG_VALUE_0"] = $"Authorization: Basic {Encode(host, token)}"
        };
    }

    /// <summary>
    /// Gets the encoded credential for a host, which is also a value that must be redacted wherever text is kept.
    /// </summary>
    /// <param name="host">The kind of host.</param>
    /// <param name="token">The access token.</param>
    /// <returns>The base64 credential.</returns>
    public static string Encode(RepositoryHost host, string token)
    {
        var user = host switch
        {
            RepositoryHost.GitHub => "x-access-token",
            RepositoryHost.GitLab => "oauth2",
            _ => string.Empty
        };

        return Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user}:{token}"));
    }
}
