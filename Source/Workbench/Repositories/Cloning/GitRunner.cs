// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Diagnostics;
using Cratis.DependencyInjection;

namespace Cratis.Prologue.Workbench.Repositories.Cloning;

/// <summary>
/// Runs the git command line as a process, without a shell and never prompting for credentials.
/// </summary>
[Singleton]
public class GitRunner : IGitRunner
{
    /// <summary>
    /// Gets the environment every git process gets: https is the only transport (credentials are only ever sent that way),
    /// git never prompts, and neither the system nor the user configuration of the Receiver's host can change what git does.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string> Hardening = new Dictionary<string, string>
    {
        ["GIT_ALLOW_PROTOCOL"] = "https",
        ["GIT_TERMINAL_PROMPT"] = "0",
        ["GIT_CONFIG_NOSYSTEM"] = "1",
        ["GIT_CONFIG_GLOBAL"] = "/dev/null"
    };

    static readonly TimeSpan _timeout = TimeSpan.FromMinutes(10);

    /// <inheritdoc/>
    public async Task<GitResult> Run(IEnumerable<string> arguments, string? workingDirectory, IReadOnlyDictionary<string, string> environment, CancellationToken cancellationToken)
    {
        var info = new ProcessStartInfo("git")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        if (workingDirectory is not null)
        {
            info.WorkingDirectory = workingDirectory;
        }

        foreach (var argument in arguments)
        {
            info.ArgumentList.Add(argument);
        }

        foreach (var (name, value) in Hardening)
        {
            info.Environment[name] = value;
        }

        foreach (var (name, value) in environment)
        {
            info.Environment[name] = value;
        }

        Process process;
        try
        {
            process = Process.Start(info) ?? throw new InvalidOperationException("git did not start");
        }
        catch (Win32Exception)
        {
            return new GitResult(127, string.Empty, "git is not installed on the Receiver");
        }

        using (process)
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(_timeout);

            var output = process.StandardOutput.ReadToEndAsync(timeout.Token);
            var error = process.StandardError.ReadToEndAsync(timeout.Token);
            try
            {
                await process.WaitForExitAsync(timeout.Token);
                return new GitResult(process.ExitCode, await output, await error);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                process.Kill(entireProcessTree: true);
                return new GitResult(124, string.Empty, "git took too long and was stopped");
            }
        }
    }
}
