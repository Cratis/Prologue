// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net.Http.Json;

namespace Cratis.Prologue.Extractor.Capturing;

/// <summary>
/// Represents an <see cref="ICaptureOutput"/> that sends correlated captures to the Prologue Receiver over HTTP. The API
/// owns persistence.
/// </summary>
/// <param name="httpClient">The <see cref="HttpClient"/> configured with the Prologue Receiver base address.</param>
/// <param name="logger">The logger.</param>
public class ApiCaptureOutput(HttpClient httpClient, ILogger<ApiCaptureOutput> logger) : ICaptureOutput
{
    /// <summary>
    /// The relative path on the Prologue Receiver captures are posted to.
    /// </summary>
    public const string CapturesPath = "captures";

    /// <inheritdoc/>
    public async Task Write(IReadOnlyList<Capture> captures, CancellationToken cancellationToken = default)
    {
        foreach (var capture in captures)
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync(CapturesPath, capture, CaptureSerialization.Options, cancellationToken);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                ApiCaptureOutputLog.SendFailed(logger, capture.Id, exception);
            }
        }
    }
}
