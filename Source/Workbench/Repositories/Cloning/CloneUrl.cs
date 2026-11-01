// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Systems;

namespace Cratis.Prologue.Workbench.Repositories.Cloning;

/// <summary>
/// Builds the address a repository is cloned from. The address never carries credentials: they are handed to git through
/// its environment when the process runs.
/// </summary>
public static class CloneUrl
{
    /// <summary>
    /// Builds the address for a request.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns>The address, or null when the host needs a connection that does not exist.</returns>
    public static string? For(CloneRequest request)
    {
        var repository = request.Repository.Value.Trim();
        if (request.Host == RepositoryHost.LocalFolder)
        {
            return repository;
        }

        if (repository.Contains("://", StringComparison.Ordinal))
        {
            return WithoutCredentials(repository);
        }

        repository = repository.Trim('/');
        if (repository.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
        {
            repository = repository[..^4];
        }

        var server = request.Server?.Value.Trim().TrimEnd('/');
        return request.Host switch
        {
            RepositoryHost.GitHub => $"{(string.IsNullOrEmpty(server) ? "https://github.com" : server)}/{repository}.git",
            RepositoryHost.GitLab => $"{(string.IsNullOrEmpty(server) ? "https://gitlab.com" : server)}/{repository}.git",
            RepositoryHost.AzureDevOps => string.IsNullOrEmpty(server) ? null : AzureDevOps(server, repository),
            _ => repository
        };
    }

    static string AzureDevOps(string organization, string repository)
    {
        var parts = repository.Split('/', 2);
        return parts.Length == 2
            ? $"{organization}/{parts[0]}/_git/{parts[1]}"
            : $"{organization}/{parts[0]}/_git/{parts[0]}";
    }

    static string WithoutCredentials(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri) && !string.IsNullOrEmpty(uri.UserInfo)
            ? new UriBuilder(uri) { UserName = string.Empty, Password = string.Empty }.Uri.ToString()
            : url;
}
