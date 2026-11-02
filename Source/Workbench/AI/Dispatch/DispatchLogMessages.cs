// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace Cratis.Prologue.Workbench.AI.Dispatch;

internal static partial class DispatchLogMessages
{
    [LoggerMessage(LogLevel.Warning, "The tokens a pool used could not be recorded, so its ceiling may count too little (pool {PoolKey})")]
    internal static partial void SpendNotRecorded(this ILogger<PoolSpendRecorder> logger, string poolKey, Exception? exception = null);

    [LoggerMessage(LogLevel.Warning, "A pool's spend could not be written and is kept for the next flush (pool {PoolKey})")]
    internal static partial void SpendKeptForLater(this ILogger<PoolSpendLedger> logger, string poolKey);
}
