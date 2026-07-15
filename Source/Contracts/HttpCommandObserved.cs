// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Contracts;

/// <summary>
/// Represents the metadata of a single state-changing HTTP command (<c>POST</c>, <c>PUT</c>, or <c>DELETE</c>)
/// observed flowing through the reverse proxy. No request or response body is captured.
/// </summary>
/// <param name="Method">The HTTP method of the request.</param>
/// <param name="Path">The request path, including query string.</param>
/// <param name="StatusCode">The HTTP status code the proxied system responded with.</param>
/// <param name="TraceId">The W3C trace id from the request's <c>traceparent</c> header, used to correlate the command with the telemetry it produces; empty when absent.</param>
public record HttpCommandObserved(string Method, string Path, int StatusCode, string TraceId = "") : ObservationPayload;
