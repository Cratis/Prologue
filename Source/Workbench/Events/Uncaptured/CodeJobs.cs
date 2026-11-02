// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Discover;
using Cratis.Prologue.Workbench.Discover.Findings;

namespace Cratis.Prologue.Workbench.Events.Uncaptured;

/// <summary>
/// Lists the jobs among what Discover found: decision points that run on a schedule and dependencies of the kind job.
/// </summary>
public static class CodeJobs
{
    /// <summary>
    /// Lists the jobs.
    /// </summary>
    /// <param name="findings">What the analyses found for a prologue.</param>
    /// <returns>The jobs, schedules first, in the order of file and line.</returns>
    public static IReadOnlyList<CodeJob> From(CodeFindings findings)
    {
        ModuleName ModuleOf(SourcePath file) =>
            findings.Capabilities.FirstOrDefault(capability => string.Equals(capability.File.Value, file.Value, StringComparison.OrdinalIgnoreCase))?.Module ?? new ModuleName(string.Empty);

        var schedules = findings.DecisionPoints
            .Where(point => point.Kind == DecisionKind.Schedule)
            .OrderBy(point => point.File.Value, StringComparer.OrdinalIgnoreCase).ThenBy(point => point.Line.Value)
            .Select(point => new CodeJob(point.Rule.Value, string.Empty, point.File, point.Line, ModuleOf(point.File)));
        var dependencies = findings.Dependencies
            .Where(dependency => dependency.Kind == DependencyKind.Job)
            .OrderBy(dependency => dependency.Target.Value, StringComparer.OrdinalIgnoreCase)
            .Select(dependency => new CodeJob(dependency.Target.Value, dependency.Detail.Value, new SourcePath(string.Empty), new LineNumber(0), new ModuleName(string.Empty)));
        return [.. schedules, .. dependencies];
    }
}
