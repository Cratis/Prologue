// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Repositories.Cloning;

/// <summary>
/// What a git process ended in.
/// </summary>
/// <param name="ExitCode">The exit code; zero is success.</param>
/// <param name="Output">What it wrote to standard output.</param>
/// <param name="Error">What it wrote to standard error.</param>
public record GitResult(int ExitCode, string Output, string Error)
{
    /// <summary>
    /// Gets a value indicating whether git succeeded.
    /// </summary>
    public bool Succeeded => ExitCode == 0;
}
