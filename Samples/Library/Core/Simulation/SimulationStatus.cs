// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Library.Core.Simulation;

/// <summary>
/// Represents how a simulation run is going, as reported to the dashboard, the frontends, and the tests.
/// </summary>
/// <param name="IsRunning">Whether a run is in progress.</param>
/// <param name="Requested">How many transactions the run was asked to carry out.</param>
/// <param name="Succeeded">How many transactions completed with a success status.</param>
/// <param name="Rejected">How many were rejected by a business rule — an expected, meaningful outcome, not a failure.</param>
/// <param name="Failed">How many failed outright (transport errors and unexpected statuses).</param>
/// <param name="StartedAt">When the run started, if it has.</param>
/// <param name="CompletedAt">When the run finished, if it has.</param>
/// <param name="LastError">The most recent failure message, if any.</param>
public record SimulationStatus(
    bool IsRunning,
    int Requested,
    int Succeeded,
    int Rejected,
    int Failed,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    string? LastError)
{
    /// <summary>
    /// Gets how many transactions have been carried out, however they turned out.
    /// </summary>
    public int Completed => Succeeded + Rejected + Failed;

    /// <summary>
    /// Gets how far through the run is, as a fraction between zero and one.
    /// </summary>
    public double Progress => Requested == 0 ? 0 : Math.Min(1, (double)Completed / Requested);
}
