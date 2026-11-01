// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Repositories.Cloning;

/// <summary>
/// Works out where a clone lives. Every system in the same repository and branch shares one directory.
/// </summary>
public static class CloneDirectories
{
    /// <summary>
    /// Gets the directory for a request.
    /// </summary>
    /// <param name="root">The configured clone directory.</param>
    /// <param name="request">The request.</param>
    /// <returns>The directory the repository is cloned into.</returns>
    public static string For(string root, CloneRequest request)
    {
        var repository = request.Repository.Value.Trim().Trim('/');
        var segments = repository.Split(['/', ':'], StringSplitOptions.RemoveEmptyEntries).Select(Sanitize);
        return Path.Combine([root, request.Host.ToString().ToLowerInvariant(), .. segments, Sanitize(request.Branch.Value)]);
    }

    static string Sanitize(string segment)
    {
        var clean = new string([.. segment.Trim().Select(character => char.IsAsciiLetterOrDigit(character) || character is '.' or '_' or '-' ? character : '_')]);
        return clean.Length == 0 || clean.All(character => character == '.') ? "_" : clean;
    }
}
