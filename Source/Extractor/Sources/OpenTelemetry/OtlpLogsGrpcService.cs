// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Extractor.Capturing;
using Grpc.Core;
using OpenTelemetry.Proto.Collector.Logs.V1;

namespace Cratis.Prologue.Extractor.Sources.OpenTelemetry;

/// <summary>
/// Represents the OTLP/gRPC log receiver. It captures log metadata from every export and forwards the request to
/// the upstream collector, acting as a transparent proxy.
/// </summary>
/// <param name="channel">The channel observations are published to.</param>
/// <param name="factory">The factory that maps log records to observations.</param>
/// <param name="forwarder">The forwarder to the upstream collector.</param>
public class OtlpLogsGrpcService(
    IObservationChannel channel,
    LogObservationFactory factory,
    GrpcLogsForwarder forwarder) : LogsService.LogsServiceBase
{
    /// <inheritdoc/>
    public override async Task<ExportLogsServiceResponse> Export(ExportLogsServiceRequest request, ServerCallContext context)
    {
        foreach (var observation in factory.ToObservations(request))
        {
            await channel.Publish(observation, context.CancellationToken);
        }

        return await forwarder.Forward(request, context.CancellationToken);
    }
}
