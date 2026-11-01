// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.AI.Usage.Daily;

namespace Cratis.Prologue.Workbench.AI.Asking;

/// <summary>
/// Answers the Cratis.AI seam for the work agents have already been given with none: the Workbench does not record agent
/// sessions yet, so every provider reads as unused and a pool picks its members by their configured order and capacity alone.
/// </summary>
public class NoRecordedAgentSessions : IRecordedAgentSessions
{
    /// <inheritdoc/>
    public Task<IEnumerable<RecordedAgentSession>> RecordedSince(DateTimeOffset since, CancellationToken cancellationToken = default) =>
        Task.FromResult<IEnumerable<RecordedAgentSession>>([]);
}
