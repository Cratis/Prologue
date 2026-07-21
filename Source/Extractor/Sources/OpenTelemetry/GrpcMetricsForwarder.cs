// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using OpenTelemetry.Proto.Collector.Metrics.V1;

namespace Cratis.Prologue.Extractor.Sources.OpenTelemetry;

/// <summary>
/// Forwards OTLP metric export requests to the upstream OTLP/gRPC collector, if one is configured. When no upstream
/// is configured the engine is a terminal sink and an empty response is returned.
/// </summary>
/// <param name="upstream">The shared channel to the upstream collector.</param>
public sealed class GrpcMetricsForwarder(OtlpUpstreamChannel upstream)
{
    readonly MetricsService.MetricsServiceClient? _client =
        upstream.Channel is null ? null : new MetricsService.MetricsServiceClient(upstream.Channel);

    /// <summary>
    /// Forwards the request to the upstream collector, or returns an empty response when no upstream is configured.
    /// </summary>
    /// <param name="request">The OTLP export request.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The upstream response, or an empty response.</returns>
    public async Task<ExportMetricsServiceResponse> Forward(ExportMetricsServiceRequest request, CancellationToken cancellationToken) =>
        _client is null
            ? new ExportMetricsServiceResponse()
            : await _client.ExportAsync(request, cancellationToken: cancellationToken);
}
