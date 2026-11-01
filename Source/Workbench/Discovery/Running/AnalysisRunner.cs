// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Discovery.Agents;
using Cratis.Prologue.Workbench.Discovery.Extraction;
using Cratis.Prologue.Workbench.Discovery.Recording;
using Cratis.Prologue.Workbench.Repositories.Cloning;

namespace Cratis.Prologue.Workbench.Discovery.Running;

/// <summary>
/// Reads a directory in two passes. First a deterministic extractor maps the controllers and routes, so a map exists without any
/// AI provider. Then the files are read in batches by the agents, which add the capabilities, decision points and dependencies
/// code alone does not show.
/// </summary>
/// <param name="agents">The Cratis.AI agents.</param>
/// <param name="progress">Where progress is reported.</param>
public class AnalysisRunner(IAnalysisAgents agents, IAnalysisProgress progress) : IAnalysisRunner
{
    const int ListedFiles = 300;

    /// <inheritdoc/>
    public async Task<AnalysisOutcome> Run(AnalysisJob job, CancellationToken cancellationToken)
    {
        var target = job.Target;
        var directory = target.Directory.Value;
        var files = SourceFileScanner.Scan(directory);
        if (files.Count == 0)
        {
            return Outcome(job, 0, AnalysisFindings.Empty, $"No source files were found under '{directory}'");
        }

        var listed = files.Take(ListedFiles).Select(_ => _.Path).ToList();
        Report(job, AnalysisState.Running, 0, listed, files.Count);

        var deterministic = new AnalysisFindings(Controllers(directory, files), [], []);
        var parts = new List<AnalysisFindings> { deterministic };
        var failures = new List<string>();
        var read = 0;

        foreach (var batchFiles in AnalysisBatches.Split(files))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var batch = new AnalysisBatch([.. batchFiles.Select(_ => new BatchFile(_.Path, Read(directory, _.Path)))]);
            var answer = await agents.Read(batch, cancellationToken);
            parts.Add(answer.Findings);
            failures.AddRange(answer.Failures);
            if (answer.Failures.Count >= 3)
            {
                break;
            }

            read += batchFiles.Count;
            Report(job, AnalysisState.Running, read, listed, files.Count);
        }

        var failure = failures.Count == 0 ? null : Describe(failures);
        return Outcome(job, read, FindingsMerge.Merge(parts), failure);
    }

    static string Describe(IEnumerable<string> failures)
    {
        var text = string.Join("; ", failures.Distinct());
        return $"The AI agents did not read everything, so nothing was removed from the map ({(text.Length > 400 ? text[..400] : text)})";
    }

    static IEnumerable<FoundCapability> Controllers(string directory, IEnumerable<SourceFile> files) =>
        files.Where(_ => _.Path.Value.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
            .SelectMany(_ => AspNetControllerExtractor.Extract(_.Path, Read(directory, _.Path)));

    static string Read(string directory, SourcePath path)
    {
        try
        {
            return File.ReadAllText(Path.Combine(directory, path.Value));
        }
        catch (IOException)
        {
            return string.Empty;
        }
    }

    static AnalysisOutcome Outcome(AnalysisJob job, int files, AnalysisFindings findings, string? failure) =>
        new(job.Target.Subject, job.Target.Name, job.Target.Source, new FileCount(files), findings, failure is null ? null : new FailureReason(failure));

    void Report(AnalysisJob job, AnalysisState state, int read, IReadOnlyList<SourcePath> listed, int total) =>
        progress.Set(new AnalysisProgress(job.Prologue, job.Target, state, read, listed, total, null));
}
