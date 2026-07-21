// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Library.Core.Telemetry;

/// <summary>
/// The library system's own instrumentation. ASP.NET Core already emits a server span per request; these add the
/// domain-level detail on top — what the system was asked to do, in its own vocabulary — which is what makes the
/// captured telemetry worth interpreting.
/// </summary>
public static class LibraryTelemetry
{
    /// <summary>
    /// The name of the activity source and meter, and the OpenTelemetry service name.
    /// </summary>
    public const string ServiceName = "Library";

    /// <summary>
    /// Gets the activity source controller-action spans are created from.
    /// </summary>
    public static readonly ActivitySource ActivitySource = new(ServiceName);

    /// <summary>
    /// Gets the meter the library's own metrics are recorded on.
    /// </summary>
    public static readonly Meter Meter = new(ServiceName);

    /// <summary>
    /// Gets the counter of business operations the system has carried out, tagged by operation and outcome.
    /// </summary>
    public static readonly Counter<long> Operations = Meter.CreateCounter<long>(
        "library.operations",
        unit: "{operation}",
        description: "Business operations carried out by the library system.");
}
