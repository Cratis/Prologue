// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Extractor.Capturing;
using Grpc.Core;
using OpenTelemetry.Proto.Collector.Trace.V1;

namespace Cratis.Prologue.Extractor.Sources.OpenTelemetry;

/// <summary>
/// Represents the OTLP/gRPC trace receiver. It captures span metadata from every export and forwards the request
/// to the upstream collector, acting as a transparent proxy.
/// </summary>
/// <param name="channel">The channel observations are published to.</param>
/// <param name="factory">The factory that maps spans to observations.</param>
/// <param name="forwarder">The forwarder to the upstream collector.</param>
public class OtlpTraceGrpcService(
    IObservationChannel channel,
    SpanObservationFactory factory,
    GrpcTraceForwarder forwarder) : TraceService.TraceServiceBase
{
    /// <inheritdoc/>
    public override async Task<ExportTraceServiceResponse> Export(ExportTraceServiceRequest request, ServerCallContext context)
    {
        foreach (var observation in factory.ToObservations(request))
        {
            await channel.Publish(observation, context.CancellationToken);
        }

        return await forwarder.Forward(request, context.CancellationToken);
    }
}
