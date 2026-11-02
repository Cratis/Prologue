// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Arc.Queries.ModelBound;
using Cratis.Prologue.Workbench.AI.Pools.Ceiling;
using MongoDB.Driver;

namespace Cratis.Prologue.Workbench.AI.Pools.Spending;

/// <summary>
/// What a provider pool used on the most recent day it was used, counted from the usage its providers reported on each answer.
/// One small document per pool: a spend recorded on a new day starts the count over, so reading the day's use never counts anything.
/// </summary>
/// <param name="Id">The pool's id.</param>
/// <param name="Day">The UTC day, as yyyy-MM-dd.</param>
/// <param name="Tokens">The tokens used on that day.</param>
[ReadModel]
public record PoolSpend(string Id, string Day, TokenAmount Tokens)
{
    /// <summary>
    /// Observes the spend of every pool that has used any.
    /// </summary>
    /// <param name="collection">The reduced spends.</param>
    /// <returns>The spends.</returns>
    public static ISubject<IEnumerable<PoolSpend>> AllPoolSpends(IMongoCollection<PoolSpend> collection) =>
        collection.Observe();

    /// <summary>
    /// Reduces a recorded spend: it adds to the day it belongs to, or starts a new day.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the pool's id.</param>
    /// <returns>The new state.</returns>
    public static PoolSpend Reduce(PoolTokensSpent @event, PoolSpend? current, EventContext context) =>
        current is not null && current.Day == @event.Day
            ? current with { Tokens = current.Tokens.Value + @event.Tokens.Value }
            : new(context.EventSourceId.Value, @event.Day, @event.Tokens);

    /// <summary>
    /// Gets the tokens used on a day.
    /// </summary>
    /// <param name="day">The UTC day, as yyyy-MM-dd.</param>
    /// <returns>The tokens; zero when the spend is from another day.</returns>
    public long On(string day) => Day == day ? Tokens.Value : 0;
}
