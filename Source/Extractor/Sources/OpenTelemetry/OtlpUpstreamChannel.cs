// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Configuration;
using Grpc.Net.Client;
using Microsoft.Extensions.Options;

namespace Cratis.Prologue.Extractor.Sources.OpenTelemetry;

/// <summary>
/// Owns the gRPC channel to the upstream OTLP collector, shared by the trace, metric, and log forwarders so the
/// three signals do not each open their own connection. When no upstream is configured the extractor is a terminal
/// sink and there is no channel.
/// </summary>
public sealed class OtlpUpstreamChannel : IDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OtlpUpstreamChannel"/> class.
    /// </summary>
    /// <param name="options">The Prologue options carrying the upstream gRPC endpoint.</param>
    public OtlpUpstreamChannel(IOptions<PrologueOptions> options)
    {
        var upstream = options.Value.OpenTelemetry.Upstream.Grpc;

        if (!string.IsNullOrWhiteSpace(upstream))
        {
            Channel = GrpcChannel.ForAddress(upstream);
        }
    }

    /// <summary>
    /// Gets the channel to the upstream collector, or <see langword="null"/> when the extractor is a terminal sink.
    /// </summary>
    public GrpcChannel? Channel { get; }

    /// <inheritdoc/>
    public void Dispose() => Channel?.Dispose();
}
