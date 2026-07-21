// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Library.Core.Simulation;
using Microsoft.AspNetCore.Mvc;

namespace Library.Core.Pages;

/// <summary>
/// The simulated load the system can be put under, and how the run in flight is going. The page refreshes itself
/// while a run is going rather than polling from script — this frontend does its data work on the server.
/// </summary>
/// <param name="api">The <see cref="LibraryApi"/> the page's handlers call.</param>
public class SimulationModel(LibraryApi api) : LibraryPageModel(api)
{
    static readonly SimulationStatus _idle = new(false, 0, 0, 0, 0, null, null, null);

    /// <summary>
    /// Gets how the current or most recent run is going.
    /// </summary>
    public SimulationStatus Status { get; private set; } = _idle;

    /// <summary>
    /// Gets or sets how many transactions the start form was filled in with.
    /// </summary>
    [BindProperty]
    public int TransactionCount { get; set; } = 10_000;

    /// <summary>
    /// Gets the run's state in a word.
    /// </summary>
    public string StatusText => Status switch
    {
        { IsRunning: true } => "Running",
        { CompletedAt: not null } => "Completed",
        _ => "Idle"
    };

    /// <summary>
    /// Gets how far through the run is, as a whole percentage.
    /// </summary>
    public int ProgressPercentage => (int)Math.Round(Status.Progress * 100, MidpointRounding.AwayFromZero);

    /// <summary>
    /// Loads the status to show.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task OnGet(CancellationToken cancellationToken)
    {
        var result = await Api.GetSimulationStatus(cancellationToken);

        Status = result.Value ?? _idle;

        if (result.Problem is { } problem)
        {
            ShowError(problem);
        }
    }

    /// <summary>
    /// Starts a run of the size the form was filled in with.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>A redirect back to the page.</returns>
    public async Task<IActionResult> OnPostStart(CancellationToken cancellationToken)
    {
        var result = await Api.StartSimulation(new StartSimulation(TransactionCount), cancellationToken);

        return AfterCommand(result.Problem);
    }

    /// <summary>
    /// Stops the run in flight.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>A redirect back to the page.</returns>
    public async Task<IActionResult> OnPostStop(CancellationToken cancellationToken)
    {
        var result = await Api.StopSimulation(cancellationToken);

        return AfterCommand(result.Problem);
    }
}
