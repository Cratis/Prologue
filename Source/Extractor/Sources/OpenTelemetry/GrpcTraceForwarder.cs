// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Configuration;
using Grpc.Net.Client;
using Microsoft.Extensions.Options;
using OpenTelemetry.Proto.Collector.Trace.V1;

namespace Cratis.Prologue.Extractor.Sources.OpenTelemetry;

/// <summary>
/// Forwards OTLP trace export requests to the upstream OTLP/gRPC collector, if one is configured. When no upstream
/// is configured the engine is a terminal sink and an empty response is returned.
/// </summary>
public sealed class GrpcTraceForwarder : IDisposable
{
    readonly GrpcChannel? _channel;
    readonly TraceService.TraceServiceClient? _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="GrpcTraceForwarder"/> class.
    /// </summary>
    /// <param name="options">The Prologue options carrying the upstream gRPC endpoint.</param>
    public GrpcTraceForwarder(IOptions<PrologueOptions> options)
    {
        var upstream = options.Value.OpenTelemetry.Upstream.Grpc;
        if (!string.IsNullOrWhiteSpace(upstream))
        {
            _channel = GrpcChannel.ForAddress(upstream);
            _client = new TraceService.TraceServiceClient(_channel);
        }
    }

    /// <summary>
    /// Forwards the request to the upstream collector, or returns an empty response when no upstream is configured.
    /// </summary>
    /// <param name="request">The OTLP export request.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The upstream response, or an empty response.</returns>
    public async Task<ExportTraceServiceResponse> Forward(ExportTraceServiceRequest request, CancellationToken cancellationToken)
    {
        if (_client is null)
        {
            return new ExportTraceServiceResponse();
        }

        return await _client.ExportAsync(request, cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public void Dispose() => _channel?.Dispose();
}
