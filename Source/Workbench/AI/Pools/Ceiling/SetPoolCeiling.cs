// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.AI.Providers.Pools;
using Cratis.Arc.Chronicle.ReadModels;
using Cratis.Arc.Validation;

namespace Cratis.Prologue.Workbench.AI.Pools.Ceiling;

/// <summary>
/// Command for setting the most tokens a provider pool may use in a day (UTC). The dispatcher stops handing the pool's agents work once
/// the day's use reaches it.
/// </summary>
/// <param name="Pool">The <see cref="AIProviderPoolId"/>.</param>
/// <param name="TokensPerDay">The ceiling.</param>
[Command]
public record SetPoolCeiling(AIProviderPoolId Pool, TokenAmount TokensPerDay) : ICanProvideEventSourceId
{
    /// <inheritdoc/>
    public EventSourceId GetEventSourceId() => Pool.Value.ToString();

    /// <summary>
    /// Handles the command. Setting the ceiling the pool already has records nothing.
    /// </summary>
    /// <param name="current">The pool's ceiling as it stands, if it has one.</param>
    /// <returns>The event, or nothing when the ceiling does not change.</returns>
    public IEnumerable<PoolCeilingSet> Handle([Unprotected("Setting twice at once keeps the last ceiling; the guard only avoids repeating the same fact")] PoolCeiling? current) =>
        current?.TokensPerDay == TokensPerDay ? [] : [new PoolCeilingSet(TokensPerDay)];
}

/// <summary>
/// Requires a ceiling to be more than zero tokens; a pool without a ceiling has it cleared instead.
/// </summary>
public class SetPoolCeilingValidator : CommandValidator<SetPoolCeiling>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetPoolCeilingValidator"/> class.
    /// </summary>
    public SetPoolCeilingValidator() => RuleFor(command => command.TokensPerDay).Must(tokens => tokens > 0).WithMessage("A ceiling must be more than zero tokens; clear it to allow any amount");
}

/// <summary>
/// Event raised when a provider pool was given a daily ceiling.
/// </summary>
/// <param name="TokensPerDay">The most tokens the pool may use in a day.</param>
[EventType]
public record PoolCeilingSet(TokenAmount TokensPerDay);
