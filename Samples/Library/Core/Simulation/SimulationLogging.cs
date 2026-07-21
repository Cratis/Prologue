// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Library.Core.Simulation;

internal static partial class SimulationLog
{
    [LoggerMessage(LogLevel.Information, "Simulating {TransactionCount} transactions across {Concurrency} workers")]
    internal static partial void Started(ILogger logger, int transactionCount, int concurrency);

    [LoggerMessage(LogLevel.Information, "Simulation finished — {Succeeded} succeeded, {Rejected} rejected, {Failed} failed")]
    internal static partial void Finished(ILogger logger, int succeeded, int rejected, int failed);

    [LoggerMessage(LogLevel.Error, "The simulation run faulted")]
    internal static partial void Faulted(ILogger logger, Exception exception);
}
