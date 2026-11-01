// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Arc.Commands;
using Cratis.Prologue.Workbench.Discovery.Recording;
using Cratis.Prologue.Workbench.Repositories.Cloning;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cratis.Prologue.Workbench.Discovery.Running;

/// <summary>
/// Runs queued analyses in the background, one at a time, and records what each found through the command pipeline. The
/// request that queued the analysis has long returned by then.
/// </summary>
/// <param name="queue">The queue.</param>
/// <param name="runner">Runs an analysis.</param>
/// <param name="progress">Where progress is reported.</param>
/// <param name="pipeline">Executes the command that records the outcome.</param>
/// <param name="logger">The logger.</param>
public class AnalysisWorker(IAnalysisQueue queue, IAnalysisRunner runner, IAnalysisProgress progress, ICommandPipeline pipeline, ILogger<AnalysisWorker> logger) : BackgroundService
{
    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            AnalysisJob job;
            try
            {
                job = await queue.Next(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            try
            {
                await Process(job, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            finally
            {
                queue.Done(job);
            }
        }
    }

    async Task Process(AnalysisJob job, CancellationToken cancellationToken)
    {
        var current = progress.Get(job.Target.Subject) ?? AnalysisProgress.Queued(job);
        progress.Set(current with { State = AnalysisState.Running });

        AnalysisOutcome outcome;
        try
        {
            outcome = await runner.Run(job, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.AnalysisCrashed(job.Target.Name.Value, exception);
            outcome = new(job.Target.Subject, job.Target.Name, job.Target.Source, new FileCount(0), AnalysisFindings.Empty, new FailureReason(exception.Message));
        }

        var recorded = await pipeline.Execute(
            new RecordCodeAnalysis(job.Prologue, outcome.Subject, outcome.Name, outcome.Source, outcome.Files, outcome.Findings.Capabilities, outcome.Findings.DecisionPoints, outcome.Findings.Dependencies, outcome.Failure));

        var reason = outcome.Failure ?? (recorded.IsSuccess ? null : new FailureReason(recorded.ValidationResults.FirstOrDefault()?.Message ?? recorded.ExceptionMessages.FirstOrDefault() ?? "Recording the analysis failed"));
        var latest = progress.Get(job.Target.Subject) ?? current;
        progress.Set(latest with { State = reason is null ? AnalysisState.Complete : AnalysisState.Failed, FilesRead = outcome.Files.Value, Reason = reason });
    }
}
