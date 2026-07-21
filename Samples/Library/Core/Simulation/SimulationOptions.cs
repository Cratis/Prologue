// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Library.Core.Simulation;

/// <summary>
/// Represents how the simulated load is generated, bound from the <c>Library:Simulation</c> configuration section.
/// </summary>
public class SimulationOptions
{
    /// <summary>
    /// The configuration section name the options are bound from.
    /// </summary>
    public const string SectionName = "Library:Simulation";

    /// <summary>
    /// The name of the <see cref="HttpClient"/> the simulated traffic is sent through.
    /// </summary>
    public const string HttpClientName = "simulation";

    /// <summary>
    /// Gets or sets where the simulated traffic is sent. This must be the Prologue Extractor's reverse proxy —
    /// traffic sent straight at the system bypasses the proxy and is never captured as an HTTP command. The Aspire
    /// composition points this at the extractor.
    /// </summary>
    public string BaseAddress { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets how many transactions a run carries out when the caller does not say.
    /// </summary>
    public int DefaultTransactionCount { get; set; } = 10_000;

    /// <summary>
    /// Gets or sets how many transactions are in flight at once.
    /// </summary>
    public int Concurrency { get; set; } = 8;

    /// <summary>
    /// Gets or sets a pause between transactions per worker, in milliseconds. Zero runs flat out; a small value
    /// spreads the load out so it looks more like a system in steady use than a burst.
    /// </summary>
    public int DelayMilliseconds { get; set; }
}
