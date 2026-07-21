// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net.Http.Json;
using Cratis.Prologue.Configuration;
using Microsoft.Extensions.Options;

namespace Cratis.Prologue.Extractor.Capturing;

/// <summary>
/// Represents an <see cref="ICaptureOutput"/> that sends correlated captures to the Prologue Receiver over HTTP. The API
/// owns persistence.
/// </summary>
/// <param name="httpClient">The <see cref="HttpClient"/> configured with the Prologue Receiver base address.</param>
/// <param name="options">The Prologue options carrying the Prologue the captures belong to.</param>
/// <param name="logger">The logger.</param>
public class ApiCaptureOutput(HttpClient httpClient, IOptions<PrologueOptions> options, ILogger<ApiCaptureOutput> logger) : ICaptureOutput
{
    /// <summary>
    /// The relative path on the Prologue Receiver captures are posted to when they belong to no Prologue.
    /// </summary>
    public const string CapturesPath = "captures";

    readonly string _path = PathFor(options.Value.PrologueId);

    /// <inheritdoc/>
    public async Task Write(IReadOnlyList<Capture> captures, CancellationToken cancellationToken = default)
    {
        foreach (var capture in captures)
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync(_path, capture, CaptureSerialization.Options, cancellationToken);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                ApiCaptureOutputLog.SendFailed(logger, capture.Id, exception);
            }
        }
    }

    /// <summary>
    /// Resolves the Receiver path captures are posted to — the Prologue-scoped endpoint when the extractor is
    /// configured with a Prologue, otherwise the unassociated one.
    /// </summary>
    /// <param name="prologueId">The Prologue the captures belong to.</param>
    /// <returns>The relative path to post to.</returns>
    static string PathFor(Guid prologueId) =>
        prologueId == Guid.Empty
            ? CapturesPath
            : $"prologues/{prologueId}/{CapturesPath}";
}
