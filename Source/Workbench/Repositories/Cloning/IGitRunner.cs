// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Repositories.Cloning;

/// <summary>
/// Runs the git command line.
/// </summary>
public interface IGitRunner
{
    /// <summary>
    /// Runs git.
    /// </summary>
    /// <param name="arguments">The arguments, each passed as is without a shell.</param>
    /// <param name="workingDirectory">The directory to run in; null for the current directory.</param>
    /// <param name="environment">Environment variables for the process only, where credentials are handed over.</param>
    /// <param name="cancellationToken">Cancels the process.</param>
    /// <returns>What git ended in.</returns>
    Task<GitResult> Run(IEnumerable<string> arguments, string? workingDirectory, IReadOnlyDictionary<string, string> environment, CancellationToken cancellationToken);
}
