// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Repositories.Cloning;

namespace Cratis.Prologue.Workbench.Repositories.given;

/// <summary>
/// A git runner that records every call and pretends to clone by creating the clone's .git directory.
/// </summary>
public class RecordingGitRunner : IGitRunner
{
    readonly List<GitCall> _calls = [];

    /// <summary>
    /// Gets or sets what a call that is not a clone answers with; by default git succeeds and the commit is abc123.
    /// </summary>
    public Func<IReadOnlyList<string>, GitResult> Answer { get; set; } = arguments =>
        arguments[0] == "rev-parse" ? new GitResult(0, "abc123\n", string.Empty) : new GitResult(0, string.Empty, string.Empty);

    /// <summary>
    /// Gets the calls made.
    /// </summary>
    public IReadOnlyList<GitCall> Calls => _calls;

    /// <summary>
    /// Gets the calls that cloned.
    /// </summary>
    public IEnumerable<GitCall> Clones => _calls.Where(call => call.Arguments[0] == "clone");

    /// <inheritdoc/>
    public Task<GitResult> Run(IEnumerable<string> arguments, string? workingDirectory, IReadOnlyDictionary<string, string> environment, CancellationToken cancellationToken)
    {
        var list = arguments.ToList();
        _calls.Add(new GitCall(list, workingDirectory, environment));
        var result = Answer(list);
        if (list[0] == "clone" && result.Succeeded)
        {
            Directory.CreateDirectory(Path.Combine(list[^1], ".git"));
        }

        return Task.FromResult(result);
    }
}

/// <summary>
/// One call to git.
/// </summary>
/// <param name="Arguments">The arguments.</param>
/// <param name="WorkingDirectory">The directory it ran in.</param>
/// <param name="Environment">The environment handed to the process.</param>
public record GitCall(IReadOnlyList<string> Arguments, string? WorkingDirectory, IReadOnlyDictionary<string, string> Environment);
#endif
