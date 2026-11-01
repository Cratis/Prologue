// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace Cratis.Prologue.Workbench.Rebuild.Parity;

/// <summary>
/// Records a parity observation by executing <see cref="RecordParityObservation"/>.
/// </summary>
/// <param name="pipeline">The pipeline commands run through.</param>
/// <param name="logger">The logger a failure to record is reported to.</param>
public class ParityRecorder(ICommandPipeline pipeline, ILogger<ParityRecorder> logger) : IParityRecorder
{
    /// <inheritdoc/>
    public async Task Record(Guid prologue, ParityObservation observation, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await pipeline.Execute(ParityMapping.ToCommand(prologue, observation));
            if (!result.IsSuccess)
            {
                logger.ParityRefused(observation.Id, result.ValidationResults.FirstOrDefault()?.Message ?? result.ExceptionMessages.FirstOrDefault() ?? result.AuthorizationFailureReason);
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.ParityFailed(observation.Id, exception);
        }
    }
}
