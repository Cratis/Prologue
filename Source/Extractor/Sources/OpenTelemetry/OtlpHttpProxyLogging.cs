// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Extractor.Sources.OpenTelemetry;

internal static partial class OtlpHttpProxyLog
{
    [LoggerMessage(LogLevel.Warning, "Failed to parse an OTLP/HTTP export at {Path}; forwarding it unparsed")]
    internal static partial void ParseFailed(ILogger logger, string path, Exception exception);

    [LoggerMessage(LogLevel.Error, "Failed to forward an OTLP/HTTP export to the upstream collector at {Path}")]
    internal static partial void ForwardFailed(ILogger logger, string path, Exception exception);
}
