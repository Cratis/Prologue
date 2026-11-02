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
    public async Task<bool> Record(Guid prologue, ParityObservation observation, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await pipeline.Execute(ParityMapping.ToCommand(prologue, observation));
            if (result.IsSuccess)
            {
                return true;
            }

            logger.ParityRefused(observation.Id, result.ValidationResults.FirstOrDefault()?.Message ?? result.ExceptionMessages.FirstOrDefault() ?? result.AuthorizationFailureReason);

            // A refusal for a reason in the observation itself is final; one caused by an exception on the way to the event store may pass.
            return !result.HasExceptions;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.ParityFailed(observation.Id, exception);
            return false;
        }
    }
}
