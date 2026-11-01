// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.AI.Usage.Daily;

namespace Cratis.Prologue.Workbench.AI.Usage;

/// <summary>
/// Answers Cratis.AI's <see cref="IRecordedAgentSessions"/>, which a pool needs to decide how much work a provider has been given
/// before it can hand out a chat client. The Workbench does not record agent sessions yet, so no work is counted against a provider.
/// </summary>
/// <remarks>
/// Registration is by convention on the name: IRecordedAgentSessions is answered by RecordedAgentSessions. Without it
/// the pool cannot be constructed and every agent call fails.
/// </remarks>
public class RecordedAgentSessions : IRecordedAgentSessions
{
    /// <inheritdoc/>
    public Task<IEnumerable<RecordedAgentSession>> RecordedSince(DateTimeOffset since, CancellationToken cancellationToken = default) =>
        Task.FromResult<IEnumerable<RecordedAgentSession>>([]);
}
