// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.AI.Providers.Pools;
using Cratis.Prologue.Workbench.AI.Pools.Ceiling;

namespace Cratis.Prologue.Workbench.AI.Pools.Spending;

/// <summary>
/// Command for recording the tokens a provider pool used on one answer. Only the dispatcher executes it.
/// </summary>
/// <param name="Pool">The <see cref="AIProviderPoolId"/>.</param>
/// <param name="Day">The UTC day the tokens were used, as yyyy-MM-dd.</param>
/// <param name="Tokens">The tokens used.</param>
[Command]
public record RecordPoolSpend(AIProviderPoolId Pool, string Day, TokenAmount Tokens) : ICanProvideEventSourceId
{
    /// <inheritdoc/>
    public EventSourceId GetEventSourceId() => Pool.Value.ToString();

    /// <summary>
    /// Handles the command. An answer that reported no tokens is not a fact and records nothing.
    /// </summary>
    /// <returns>The event, or nothing when no tokens were used.</returns>
    public IEnumerable<PoolTokensSpent> Handle() => Tokens.Value > 0 ? [new PoolTokensSpent(Day, Tokens)] : [];
}

/// <summary>
/// Event raised when a provider pool used tokens answering an agent.
/// </summary>
/// <param name="Day">The UTC day the tokens were used, as yyyy-MM-dd.</param>
/// <param name="Tokens">The tokens used.</param>
[EventType]
public record PoolTokensSpent(string Day, TokenAmount Tokens);
