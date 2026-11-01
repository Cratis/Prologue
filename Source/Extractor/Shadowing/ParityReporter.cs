// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net.Http.Json;

namespace Cratis.Prologue.Extractor.Shadowing;

/// <summary>
/// Posts parity observations to <c language="csharp">POST /prologues/{prologueId}/parity</c> on the Receiver.
/// </summary>
/// <param name="httpClient">The client configured with the Receiver's base address.</param>
/// <param name="prologueId">The Prologue the observations belong to.</param>
/// <param name="logger">The logger.</param>
public class ParityReporter(HttpClient httpClient, Guid prologueId, ILogger<ParityReporter> logger) : IParityReporter
{
    /// <summary>
    /// Gets the path, relative to the Receiver's address, observations of a Prologue are posted to.
    /// </summary>
    /// <param name="prologueId">The Prologue.</param>
    /// <returns>The relative path.</returns>
    public static string PathFor(Guid prologueId) => $"prologues/{prologueId}/parity";

    /// <inheritdoc/>
    public async Task Report(ParityObservation observation, CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(ShadowLimits.ReportTimeout);

        try
        {
            var response = await httpClient.PostAsJsonAsync(PathFor(prologueId), observation, CaptureSerialization.Options, timeout.Token);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception exception) when (exception is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            ShadowingLog.ReportFailed(logger, observation.Id, exception);
        }
    }
}
