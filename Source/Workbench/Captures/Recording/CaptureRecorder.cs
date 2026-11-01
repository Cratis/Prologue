// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace Cratis.Prologue.Workbench.Captures.Recording;

/// <summary>
/// Records a stored capture by executing <see cref="RecordCapture"/>.
/// </summary>
/// <param name="pipeline">The pipeline commands run through.</param>
/// <param name="logger">The logger a failure to record is reported to.</param>
public class CaptureRecorder(ICommandPipeline pipeline, ILogger<CaptureRecorder> logger) : ICaptureRecorder
{
    /// <inheritdoc/>
    public async Task Record(Capture capture, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await pipeline.Execute(CaptureMapping.ToCommand(capture));
            if (!result.IsSuccess)
            {
                logger.RecordingRefused(capture.Id, FailureReason(result));
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.RecordingFailed(capture.Id, exception);
        }
    }

    static string FailureReason(CommandResult result) =>
        result.ValidationResults.FirstOrDefault()?.Message
        ?? result.ExceptionMessages.FirstOrDefault()
        ?? result.AuthorizationFailureReason;
}
