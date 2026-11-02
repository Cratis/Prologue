// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Prologue.Workbench.Common.Recording;

/// <summary>
/// Runs one recording in a scope of its own and stops waiting for it after a time limit, so one item that never completes
/// (the Chronicle command pipeline waits for an unreachable server) cannot stall the queue behind it.
/// </summary>
public static class BoundedRecording
{
    /// <summary>
    /// Runs <paramref name="record"/> in a new scope and waits at most <paramref name="timeout"/> for it.
    /// </summary>
    /// <param name="scopes">Creates the scope the recording runs in.</param>
    /// <param name="timeout">How long to wait for the recording.</param>
    /// <param name="record">The recording, given the scope's services.</param>
    /// <param name="cancellationToken">Cancelled when the worker stops.</param>
    /// <returns><see langword="true"/> if the recording finished (successfully or not) in time; <see langword="false"/> if it timed out and was abandoned.</returns>
    public static async Task<bool> Run(IServiceScopeFactory scopes, TimeSpan timeout, Func<IServiceProvider, Task> record, CancellationToken cancellationToken)
    {
        var scope = scopes.CreateAsyncScope();
        Task running;
        try
        {
            running = record(scope.ServiceProvider);
        }
        catch
        {
            await scope.DisposeAsync();
            throw;
        }

        try
        {
            await running.WaitAsync(timeout, cancellationToken);
        }
        catch (TimeoutException)
        {
            // The abandoned call may still be using the scope; release it only when that call ends.
            _ = running.ContinueWith(_ => scope.DisposeAsync().AsTask(), TaskScheduler.Default);
            return false;
        }
        catch
        {
            await scope.DisposeAsync();
            throw;
        }

        await scope.DisposeAsync();
        return true;
    }
}
