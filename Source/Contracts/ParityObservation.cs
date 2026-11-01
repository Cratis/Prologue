// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Contracts;

/// <summary>
/// What the Extractor reports to the Receiver after it mirrored a command to the shadow system: the two statuses and whether the
/// systems decided the same way. It never holds a request or response body.
/// </summary>
/// <param name="Id">The identity of the observation, used to correlate it with the capture of the same request.</param>
/// <param name="Occurred">When the legacy system answered.</param>
/// <param name="Method">The HTTP method.</param>
/// <param name="Path">The request path, without its query string.</param>
/// <param name="Shape">The shape of the command, in the same format as a captured shape.</param>
/// <param name="Trace">The W3C trace id the request carried, or empty.</param>
/// <param name="LegacyStatus">The status the legacy system answered with.</param>
/// <param name="ShadowStatus">The status the shadow system answered with; 0 when it did not answer.</param>
/// <param name="Outcome">How the two compare.</param>
public record ParityObservation(
    Guid Id,
    DateTimeOffset Occurred,
    string Method,
    string Path,
    string Shape,
    string Trace,
    int LegacyStatus,
    int ShadowStatus,
    ParityOutcome Outcome);
