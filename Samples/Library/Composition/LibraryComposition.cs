// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Library.Composition;

/// <summary>
/// The names and defaults the composition and its commands agree on.
/// </summary>
public static class LibraryComposition
{
    /// <summary>
    /// The name of the default HTTP endpoint an Aspire project resource exposes.
    /// </summary>
    public const string HttpEndpointName = "http";

    /// <summary>
    /// The name of the extractor endpoint the captured system's traffic flows through.
    /// </summary>
    /// <remarks>
    /// These are the Kestrel endpoint names from the extractor's own appsettings.json, carried over verbatim.
    /// Aspire derives a project's endpoints from Kestrel configuration when it is present and keeps the names as
    /// written, so the composition adopts them rather than declaring endpoints of its own — declaring them again
    /// fails at startup with "Endpoint with name 'Proxy' already exists".
    /// </remarks>
    public const string ProxyEndpointName = "Proxy";

    /// <summary>
    /// The name of the extractor's OTLP/HTTP receiver endpoint.
    /// </summary>
    public const string OtlpHttpEndpointName = "OtlpHttp";

    /// <summary>
    /// The name of the extractor's OTLP/gRPC receiver endpoint.
    /// </summary>
    public const string OtlpGrpcEndpointName = "OtlpGrpc";

    /// <summary>
    /// The ports the extractor listens on while orchestrated here.
    /// </summary>
    /// <remarks>
    /// The extractor's own appsettings.json uses 8080 for the proxy and the standard 4317/4318 for OTLP, which is
    /// right for running it standalone or as a container. On a developer machine those are heavily contended —
    /// Docker Desktop alone commonly holds 8080 and 4317 — and losing the race means Kestrel cannot bind, the
    /// process dies at startup, and the endpoint proxy is left accepting connections with nothing behind it, which
    /// reads from the outside as a proxy that merely hangs.
    /// <para>
    /// Aspire takes its target ports from the project's Kestrel configuration and never rewrites them, so the two
    /// sides have to be told the same thing: these values are set as the endpoints' target ports and handed to
    /// Kestrel as environment overrides. They are offset well clear of the conventional ones for that reason.
    /// </para>
    /// </remarks>
    public const int ProxyPort = 18080;

    /// <summary>
    /// The port the extractor's OTLP/HTTP receiver listens on while orchestrated here.
    /// </summary>
    public const int OtlpHttpPort = 14318;

    /// <summary>
    /// The port the extractor's OTLP/gRPC receiver listens on while orchestrated here.
    /// </summary>
    public const int OtlpGrpcPort = 14317;

    /// <summary>
    /// How many transactions a simulation run carries out unless the operator says otherwise.
    /// </summary>
    public const int DefaultTransactionCount = 10_000;

    /// <summary>
    /// The Prologue the captures belong to. Fixed rather than generated so repeated runs accumulate against the
    /// same Prologue and can be interpreted together.
    /// </summary>
    public const string DefaultPrologueId = "8f6b1c40-5d2e-4a71-9c3f-0b7a5e21d4c8";
}
