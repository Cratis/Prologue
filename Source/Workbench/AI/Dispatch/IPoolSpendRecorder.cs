// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.AI.Dispatch;

/// <summary>
/// Writes what a pool spent to the event log. It is called with a sum, in batches, never once per answer.
/// </summary>
public interface IPoolSpendRecorder
{
    /// <summary>
    /// Records the tokens a pool used on a day.
    /// </summary>
    /// <param name="poolKey">The pool's id.</param>
    /// <param name="day">The UTC day the tokens were used, as yyyy-MM-dd.</param>
    /// <param name="tokens">The tokens used since the last record.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>True when the spend is recorded; false when it is not, so the caller keeps it for the next attempt.</returns>
    Task<bool> Record(string poolKey, string day, long tokens, CancellationToken cancellationToken);
}
