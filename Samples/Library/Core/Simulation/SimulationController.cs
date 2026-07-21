// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Mvc;

namespace Library.Core.Simulation;

/// <summary>
/// Represents a request to simulate a system under use.
/// </summary>
/// <param name="TransactionCount">How many transactions to carry out; the configured default when not positive.</param>
public record StartSimulation(int TransactionCount);

/// <summary>
/// Starts, stops, and reports on simulated load. The Aspire dashboard's custom command on the Core service posts
/// here, as do the frontends and the tests.
/// </summary>
/// <param name="runner">The runner that carries out the simulation.</param>
[ApiController]
[Route("api/simulation")]
public class SimulationController(SimulationRunner runner) : ControllerBase
{
    /// <summary>
    /// Reports how the current or most recent run is going.
    /// </summary>
    /// <returns>The simulation status.</returns>
    [HttpGet("status")]
    public ActionResult<SimulationStatus> Status() => runner.Status;

    /// <summary>
    /// Starts a simulation run.
    /// </summary>
    /// <param name="command">How many transactions to carry out.</param>
    /// <returns>The status of the started run, or conflict when one is already in flight.</returns>
    [HttpPost("start")]
    public ActionResult<SimulationStatus> Start([FromBody] StartSimulation command)
    {
        if (!runner.Start(command.TransactionCount))
        {
            return Conflict(new ProblemDetails
            {
                Title = "A simulation is already running",
                Detail = "Wait for the run in flight to finish, or stop it first.",
                Status = StatusCodes.Status409Conflict
            });
        }

        return Accepted(runner.Status);
    }

    /// <summary>
    /// Stops the run in flight.
    /// </summary>
    /// <returns>The status once the run has unwound.</returns>
    [HttpPost("stop")]
    public async Task<ActionResult<SimulationStatus>> Stop()
    {
        await runner.Stop();
        return runner.Status;
    }
}
