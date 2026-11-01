// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Extractor.Shadowing;

/// <summary>
/// Sends a copy of a request to the shadow destination: method, path, query, the headers <see cref="ShadowHeaders"/> chose and the body.
/// The answer's body is never read.
/// </summary>
/// <param name="httpClient">The client; it must not follow redirects, so a redirect is compared as the decision it is.</param>
/// <param name="logger">The logger.</param>
public class ShadowSender(HttpClient httpClient, ILogger<ShadowSender> logger) : IShadowSender
{
    /// <inheritdoc/>
    public async Task<int?> Send(ShadowJob job, CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(ShadowLimits.ShadowTimeout);

        try
        {
            using var request = Build(job);
            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
            return (int)response.StatusCode;
        }
        catch (Exception exception) when (exception is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            ShadowingLog.ShadowUnavailable(logger, job.Rule.Label, job.Rule.Destination.Host, exception);
            return null;
        }
    }

    static HttpRequestMessage Build(ShadowJob job)
    {
        var baseAddress = job.Rule.Destination.ToString().TrimEnd('/');
        var request = new HttpRequestMessage(new HttpMethod(job.Method), $"{baseAddress}{job.Path}{job.Query}");
        if (job.Body is { Length: > 0 })
        {
            request.Content = new ByteArrayContent(job.Body);
        }

        foreach (var (name, values) in job.Headers)
        {
            if (!request.Headers.TryAddWithoutValidation(name, values))
            {
                request.Content?.Headers.TryAddWithoutValidation(name, values);
            }
        }

        return request;
    }
}
