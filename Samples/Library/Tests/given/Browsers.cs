// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// The test host generates its own entry-point 'Program' in the global namespace, which shadows Playwright's
// command-line driver — the alias keeps the driver reachable.
using PlaywrightCli = Microsoft.Playwright.Program;

namespace Library.Tests.given;

/// <summary>
/// Makes sure Playwright has a browser to drive. The browser builds are downloaded rather than shipped with the
/// package, so the first run on a machine installs Chromium — and a machine that cannot reach the download says so
/// rather than failing with something that looks like the frontends being broken.
/// </summary>
public static class Browsers
{
    static readonly Lazy<string?> _unavailableReason = new(Install, LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>
    /// Gets why Chromium cannot be driven, or <c>null</c> when it is installed and ready.
    /// </summary>
    public static string? UnavailableReason => _unavailableReason.Value;

    static string? Install()
    {
        try
        {
            // RunWithResult rather than Run: the driver's output is captured instead of inherited, which is the
            // only way to see why an install failed from inside a test host.
            var result = new PlaywrightCli().RunWithResult(["install", "chromium"]);

            return result.ExitCode == 0
                ? null
                : $"Playwright could not install Chromium (exit code {result.ExitCode}): {result.StandardError}";
        }
        catch (Exception error)
        {
            return $"Playwright could not install Chromium ({error.Message}), so neither frontend can be driven.";
        }
    }
}
