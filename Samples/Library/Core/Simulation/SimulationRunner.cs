// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Options;

namespace Library.Core.Simulation;

/// <summary>
/// Drives a simulation run — a fixed number of transactions carried out by a handful of concurrent workers against
/// the running system, through the Prologue Extractor's proxy. One run at a time; asking for another while one is
/// in flight is refused rather than queued.
/// </summary>
/// <param name="httpClientFactory">The factory for the client traffic is sent through.</param>
/// <param name="options">The simulation options.</param>
/// <param name="logger">The logger.</param>
public sealed class SimulationRunner(
    IHttpClientFactory httpClientFactory,
    IOptions<SimulationOptions> options,
    ILogger<SimulationRunner> logger) : IDisposable
{
    readonly Lock _sync = new();
    CancellationTokenSource? _cancellation;
    Task? _run;
    int _requested;
    int _succeeded;
    int _rejected;
    int _failed;
    DateTimeOffset? _startedAt;
    DateTimeOffset? _completedAt;
    string? _lastError;

    /// <summary>
    /// Gets how the current or most recent run is going.
    /// </summary>
    public SimulationStatus Status
    {
        get
        {
            lock (_sync)
            {
                return new SimulationStatus(
                    _run is { IsCompleted: false },
                    _requested,
                    Volatile.Read(ref _succeeded),
                    Volatile.Read(ref _rejected),
                    Volatile.Read(ref _failed),
                    _startedAt,
                    _completedAt,
                    _lastError);
            }
        }
    }

    /// <summary>
    /// Starts a run.
    /// </summary>
    /// <param name="transactionCount">How many transactions to carry out; the configured default when not positive.</param>
    /// <returns>True when the run started; false when one is already in flight.</returns>
    public bool Start(int transactionCount)
    {
        lock (_sync)
        {
            if (_run is { IsCompleted: false })
            {
                return false;
            }

            var count = transactionCount > 0 ? transactionCount : options.Value.DefaultTransactionCount;

            _cancellation?.Dispose();
            _cancellation = new CancellationTokenSource();
            _requested = count;
            _succeeded = 0;
            _rejected = 0;
            _failed = 0;
            _startedAt = DateTimeOffset.UtcNow;
            _completedAt = null;
            _lastError = null;

            _run = Run(count, _cancellation.Token);

            SimulationLog.Started(logger, count, options.Value.Concurrency);

            return true;
        }
    }

    /// <summary>
    /// Stops the run in flight, if any, and waits for its workers to unwind.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task Stop()
    {
        Task? run;
        CancellationTokenSource? cancellation;

        lock (_sync)
        {
            run = _run;
            cancellation = _cancellation;
        }

        if (cancellation is not null)
        {
            await cancellation.CancelAsync();
        }

        if (run is not null)
        {
            await run;
        }
    }

    /// <inheritdoc/>
    public void Dispose() => _cancellation?.Dispose();

    async Task Run(int transactionCount, CancellationToken cancellationToken)
    {
        // Yield first so Start() returns to its caller before the run gets going and takes the lock's work with it.
        await Task.Yield();

        var client = httpClientFactory.CreateClient(SimulationOptions.HttpClientName);
        var scenario = new LibraryScenario(client);
        var remaining = transactionCount;

        try
        {
            await scenario.Prime(cancellationToken);

            var workers = Enumerable
                .Range(0, Math.Max(1, options.Value.Concurrency))
                .Select(worker => Worker(scenario, worker, () => Interlocked.Decrement(ref remaining) >= 0, cancellationToken));

            await Task.WhenAll(workers);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _lastError = exception.Message;
            SimulationLog.Faulted(logger, exception);
        }
        finally
        {
            lock (_sync)
            {
                _completedAt = DateTimeOffset.UtcNow;
            }

            SimulationLog.Finished(logger, Volatile.Read(ref _succeeded), Volatile.Read(ref _rejected), Volatile.Read(ref _failed));
        }
    }

    async Task Worker(LibraryScenario scenario, int seed, Func<bool> tryClaim, CancellationToken cancellationToken)
    {
        var random = new Random(seed * 7919);
        var delay = options.Value.DelayMilliseconds;

        while (!cancellationToken.IsCancellationRequested && tryClaim())
        {
            try
            {
                var outcome = await scenario.ExecuteOne(random, cancellationToken);
                Record(outcome);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                Interlocked.Increment(ref _failed);
                _lastError = exception.Message;
            }

            if (delay > 0)
            {
                await Task.Delay(delay, cancellationToken);
            }
        }
    }

    void Record(LibraryScenario.Outcome outcome)
    {
        switch (outcome)
        {
            case LibraryScenario.Outcome.Succeeded:
                Interlocked.Increment(ref _succeeded);
                break;

            case LibraryScenario.Outcome.Rejected:
                Interlocked.Increment(ref _rejected);
                break;

            default:
                Interlocked.Increment(ref _failed);
                break;
        }
    }
}
