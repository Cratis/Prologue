// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Extractor.Shadowing;

/// <summary>
/// One request to mirror to the shadow destination, taken after the legacy system answered.
/// </summary>
/// <param name="Id">The identity of the observation it will produce.</param>
/// <param name="Rule">The rule that matched.</param>
/// <param name="Occurred">When the legacy system answered.</param>
/// <param name="Method">The HTTP method.</param>
/// <param name="Path">The request path as it was sent (percent-encoded), without its query string.</param>
/// <param name="Query">The query string, with its leading question mark, or empty.</param>
/// <param name="Shape">The shape of the command.</param>
/// <param name="Trace">The trace id the request carried, or empty.</param>
/// <param name="Headers">The headers to copy.</param>
/// <param name="Body">The body to copy, or null when the request had none or it was too large to copy.</param>
/// <param name="LegacyStatus">The status the legacy system answered with.</param>
/// <param name="BodySkipped">Whether the body was too large to copy, so the request is reported as skipped instead of mirrored.</param>
public record ShadowJob(
    Guid Id,
    ShadowRule Rule,
    DateTimeOffset Occurred,
    string Method,
    string Path,
    string Query,
    string Shape,
    string Trace,
    IReadOnlyList<KeyValuePair<string, string[]>> Headers,
    byte[]? Body,
    int LegacyStatus,
    bool BodySkipped);
