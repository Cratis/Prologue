// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Diagnostics;

namespace Library.Tests.given;

/// <summary>
/// Probes for a container runtime the distributed application can be hosted on. Everything here needs MongoDB and a
/// relational database in containers, so a machine without a running daemon cannot host the run at all — the specs
/// skip with the reason rather than fail on something the code under test has no say in.
/// </summary>
public static class ContainerRuntime
{
    static readonly Lazy<string?> _unavailableReason = new(Probe, LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>
    /// Gets why the container runtime cannot be used, or <c>null</c> when it is available.
    /// </summary>
    public static string? UnavailableReason => _unavailableReason.Value;

    static string? Probe()
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo("docker", "info")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            });

            if (process is null)
            {
                return "Docker could not be started, so the distributed application has nothing to run its containers on.";
            }

            if (!process.WaitForExit(60_000))
            {
                process.Kill(entireProcessTree: true);
                return "'docker info' did not answer within a minute, so no usable container runtime was found.";
            }

            return process.ExitCode == 0
                ? null
                : "'docker info' reported a failure, so no container runtime is running to host MongoDB and the library database.";
        }
        catch (Exception error) when (error is Win32Exception or InvalidOperationException or PlatformNotSupportedException)
        {
            return $"Docker is not installed or not reachable on this machine ({error.Message}), so the distributed application cannot be started.";
        }
    }
}
