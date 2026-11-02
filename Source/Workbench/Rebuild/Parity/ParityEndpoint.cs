// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Http;

namespace Cratis.Prologue.Workbench.Rebuild.Parity;

/// <summary>
/// The answer to an Extractor reporting a parity observation.
/// </summary>
public static class ParityEndpoint
{
    /// <summary>
    /// Queues the observation and answers. A full queue answers 503: the Extractor treats that as a failed report and logs it, which tells the
    /// truth where a 202 would have claimed an observation that is then lost. Capture endpoints keep answering 202 because they store the capture
    /// before they queue it; only its recording for the Workbench is at stake there, and a 503 would make the Extractor repost a capture that is stored.
    /// </summary>
    /// <param name="prologue">The prologue the Extractor is configured for.</param>
    /// <param name="observation">The observation.</param>
    /// <param name="recordings">The queue observations wait in.</param>
    /// <returns>202 when queued or a retry of one that was; 503 when the queue is full.</returns>
    public static IResult Accept(Guid prologue, ParityObservation observation, IParityRecordingQueue recordings) =>
        recordings.TryEnqueue(prologue, observation) ? Results.Accepted() : Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
}
