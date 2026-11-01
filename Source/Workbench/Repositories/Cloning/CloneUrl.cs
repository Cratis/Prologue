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

    /// <summary>
    /// Gets the reason a request must not be cloned from the address built for it, or null when it can be.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="url">The address built for the request.</param>
    /// <returns>The reason, or null.</returns>
    public static string? Refusal(CloneRequest request, string url)
    {
        if (request.Host != RepositoryHost.LocalFolder && IsLocalReference(request.Repository.Value.Trim()))
        {
            return "Only a LocalFolder system can name a folder on the Receiver; give the repository as owner/name or an https address";
        }

        return Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps
            ? null
            : "Repositories are cloned over https only; give the repository as owner/name or an https address";
    }

    /// <summary>
    /// Gets the host the access token of the request's connection belongs to.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns>The host name; null when the connection does not say.</returns>
    public static string? ConnectionHost(CloneRequest request)
    {
        var server = request.Server?.Value.Trim();
        if (!string.IsNullOrEmpty(server))
        {
            return Uri.TryCreate(server, UriKind.Absolute, out var uri) ? uri.Host : null;
        }

        return request.Host switch
        {
            RepositoryHost.GitHub => "github.com",
            RepositoryHost.GitLab => "gitlab.com",
            _ => null
        };
    }

    /// <summary>
    /// Gets whether the access token of the request's connection may be sent to the address: it has to be https and on the
    /// host the connection belongs to, so a repository address cannot lead the token to another server.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="url">The address built for the request.</param>
    /// <returns>True when the token may be sent.</returns>
    public static bool MayCarryToken(CloneRequest request, string url) =>
        ConnectionHost(request) is { } host
        && Uri.TryCreate(url, UriKind.Absolute, out var uri)
        && uri.Scheme == Uri.UriSchemeHttps
        && string.Equals(uri.Host, host, StringComparison.OrdinalIgnoreCase);

    static bool IsLocalReference(string repository) =>
        repository.StartsWith('.')
        || repository.StartsWith('~')
        || repository.Contains('\\', StringComparison.Ordinal)
        || repository.Contains("::", StringComparison.Ordinal)
        || repository.StartsWith("file:", StringComparison.OrdinalIgnoreCase)
        || (repository.Length > 1 && repository[1] == ':' && char.IsLetter(repository[0]) && !repository.Contains("://", StringComparison.Ordinal));

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
