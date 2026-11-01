// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Discovery.Findings;
using Cratis.Prologue.Workbench.Discovery.Running;
using Cratis.Prologue.Workbench.Prologues;
using Cratis.Prologue.Workbench.Repositories.Cloning;
using Cratis.Prologue.Workbench.Systems;

namespace Cratis.Prologue.Workbench.Discovery.Map;

/// <summary>
/// Works out the analysis status of the systems and folders of a prologue.
/// </summary>
public static class AnalysisStatuses
{
    /// <summary>
    /// Works out the status of every system and folder. What is running wins over what was recorded; a system that was never
    /// analyzed is listed so it can be started.
    /// </summary>
    /// <param name="prologue">The prologue.</param>
    /// <param name="findings">What the analyses found.</param>
    /// <param name="definition">The prologue's definition.</param>
    /// <param name="running">The progress of analyses since the Receiver started.</param>
    /// <returns>The statuses, systems first and then folders.</returns>
    public static IEnumerable<AnalysisStatus> Build(PrologueId prologue, CodeFindings findings, PrologueDefinition? definition, IEnumerable<AnalysisProgress> running)
    {
        var current = running.Where(_ => _.Prologue == prologue).ToDictionary(_ => _.Target.Subject);
        var statuses = new List<AnalysisStatus>();

        foreach (var system in definition?.Systems.Where(_ => _.Location is not null) ?? [])
        {
            var subject = AnalysisSubjectId.For(system.Id);
            statuses.Add(current.TryGetValue(subject, out var progress) ? FromProgress(progress) : FromRun(subject, new SubjectName(system.Name.Value), system.Location!, findings.RunOf(subject)));
        }

        var listed = statuses.Select(_ => _.Subject).ToHashSet();
        foreach (var progress in current.Values.Where(_ => !listed.Contains(_.Target.Subject)))
        {
            statuses.Add(FromProgress(progress));
            listed.Add(progress.Target.Subject);
        }

        foreach (var run in findings.Runs.Where(_ => !listed.Contains(_.Subject)))
        {
            var location = new SystemCodeLocation(RepositoryHost.LocalFolder, new RepositoryName(run.Source?.Value ?? string.Empty), new BranchName(string.Empty), new SubPath(string.Empty));
            statuses.Add(FromRun(run.Subject, run.Name, location, run));
        }

        return statuses;
    }

    static AnalysisStatus FromProgress(AnalysisProgress progress) =>
        new(
            progress.Target.Subject,
            progress.Target.Name,
            progress.State,
            new FileCount(progress.FilesRead),
            new FileCount(progress.TotalFiles),
            progress.Target.Host,
            progress.Target.Repository,
            progress.Target.Branch,
            progress.Target.SubPath,
            progress.Target.Source,
            progress.Reason,
            progress.Files);

    static AnalysisStatus FromRun(AnalysisSubjectId subject, SubjectName name, SystemCodeLocation location, AnalysisRun? run)
    {
        var state = AnalysisState.NotAnalyzed;
        if (run is not null)
        {
            state = run.Reason is null ? AnalysisState.Complete : AnalysisState.Failed;
        }

        return new(subject, name, state, run?.Files ?? new FileCount(0), run?.Files ?? new FileCount(0), location.Host, location.Repository, location.Branch, location.SubPath, run?.Source, run?.Reason, []);
    }
}
