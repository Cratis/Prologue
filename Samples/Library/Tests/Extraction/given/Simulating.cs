// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Library.Tests.Extraction.given;

/// <summary>
/// How a simulation run reports itself. Mirrors the API rather than referencing the system's own type, so the specs
/// hold the API to its published shape.
/// </summary>
/// <param name="IsRunning">Whether a run is in flight.</param>
/// <param name="Requested">How many transactions the run was asked to carry out.</param>
/// <param name="Succeeded">How many completed successfully.</param>
/// <param name="Rejected">How many a business rule turned down — an expected outcome, not a failure.</param>
/// <param name="Failed">How many failed outright.</param>
public record SimulationSnapshot(bool IsRunning, int Requested, int Succeeded, int Rejected, int Failed);

/// <summary>
/// Drives the simulation endpoints through the extractor's proxy, which is the only path where the traffic is seen.
/// </summary>
public static class Simulating
{
    static readonly JsonSerializerOptions _camelCase = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Asks for a run of the given size.
    /// </summary>
    /// <param name="client">The client pointed at the extractor's proxy.</param>
    /// <param name="transactionCount">How many transactions to carry out.</param>
    /// <returns>The response, so the caller can tell an accepted run from a refused one.</returns>
    public static Task<HttpResponseMessage> Start(this HttpClient client, int transactionCount) =>
        client.PostAsJsonAsync("/api/simulation/start", new { transactionCount }, _camelCase);

    /// <summary>
    /// Stops whatever run is in flight.
    /// </summary>
    /// <param name="client">The client pointed at the extractor's proxy.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task Stop(this HttpClient client)
    {
        using var response = await client.PostAsync(new Uri("/api/simulation/stop", UriKind.Relative), content: null);
    }

    /// <summary>
    /// Reads how the current or most recent run is going.
    /// </summary>
    /// <param name="client">The client pointed at the extractor's proxy.</param>
    /// <returns>The <see cref="SimulationSnapshot"/>.</returns>
    /// <exception cref="SimulationUnreadable">Thrown when the status could not be read.</exception>
    public static async Task<SimulationSnapshot> Status(this HttpClient client)
    {
        using var response = await client.GetAsync(new Uri("/api/simulation/status", UriKind.Relative));

        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new SimulationUnreadable(response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<SimulationSnapshot>(_camelCase)
            ?? throw new SimulationUnreadable(response.StatusCode);
    }

    /// <summary>
    /// Waits for the run in flight to finish.
    /// </summary>
    /// <param name="client">The client pointed at the extractor's proxy.</param>
    /// <param name="patience">How long to give it.</param>
    /// <returns>The final <see cref="SimulationSnapshot"/>, still running when it ran out of time.</returns>
    public static async Task<SimulationSnapshot> WaitUntilDone(this HttpClient client, TimeSpan patience)
    {
        var deadline = DateTimeOffset.UtcNow + patience;
        var snapshot = await client.Status();

        while (snapshot.IsRunning && DateTimeOffset.UtcNow < deadline)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            snapshot = await client.Status();
        }

        return snapshot;
    }
}
