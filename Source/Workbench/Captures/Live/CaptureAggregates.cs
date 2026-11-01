// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.ReadModels;
using Cratis.Prologue.Workbench.Captures.Recording;
using Cratis.Prologue.Workbench.Common;
using Cratis.Prologue.Workbench.Prologues;
using Cratis.Prologue.Workbench.Scenarios;

namespace Cratis.Prologue.Workbench.Captures.Live;

/// <summary>
/// The numbers a prologue's Capture page shows, kept up to date as each capture arrives so that nothing is counted when a page opens.
/// </summary>
/// <param name="Id">The prologue.</param>
/// <param name="Total">All captures.</param>
/// <param name="WithCommand">The captures that hold an HTTP command.</param>
/// <param name="TraceLinked">The captures with a command whose spans were joined by trace.</param>
/// <param name="UncorrelatedTransactions">The database transactions that arrived with no command.</param>
/// <param name="Tables">The tables captures changed.</param>
/// <param name="PerMinute">The last <see cref="MinutesKept"/> minutes with captures, oldest first, for the rate.</param>
/// <param name="Scenarios">The captures per scenario label.</param>
[ReadModel]
public record CaptureAggregates(
    PrologueId Id,
    int Total,
    int WithCommand,
    int TraceLinked,
    int UncorrelatedTransactions,
    IEnumerable<TableTouched> Tables,
    IEnumerable<MinuteBucket> PerMinute,
    IEnumerable<ScenarioCaptures> Scenarios)
{
    /// <summary>
    /// The most minutes kept for the rate.
    /// </summary>
    public const int MinutesKept = 60;

    /// <summary>
    /// Observes the aggregates of a prologue.
    /// </summary>
    /// <param name="id">The prologue.</param>
    /// <param name="readModels">Observes the stored aggregates.</param>
    /// <returns>The aggregates, pushed again whenever a capture arrives.</returns>
    public static ISubject<CaptureAggregates> ObserveCaptureAggregates(PrologueId id, IReadModels readModels) =>
        readModels.Observe<CaptureAggregates>(aggregates => aggregates.Id == id).FirstItem();

    /// <summary>
    /// Reduces a capture arriving.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue's identity.</param>
    /// <returns>The new state.</returns>
    public static CaptureAggregates Reduce(CaptureRecorded @event, CaptureAggregates? current, EventContext context)
    {
        var aggregates = current ?? new(new PrologueId(Guid.Parse(context.EventSourceId.Value)), 0, 0, 0, 0, [], [], []);
        var hasCommand = !@event.Http.Shape.IsNone;

        return aggregates with
        {
            Total = aggregates.Total + 1,
            WithCommand = aggregates.WithCommand + (hasCommand ? 1 : 0),
            TraceLinked = aggregates.TraceLinked + (hasCommand && @event.TraceLinked ? 1 : 0),
            UncorrelatedTransactions = aggregates.UncorrelatedTransactions + (hasCommand ? 0 : @event.Transactions.Count()),
            Tables = AddTables(aggregates.Tables, @event),
            PerMinute = AddToMinute(aggregates.PerMinute, @event.Occurred),
            Scenarios = AddToScenario(aggregates.Scenarios, @event.Scenario)
        };
    }

    static IEnumerable<TableTouched> AddTables(IEnumerable<TableTouched> tables, CaptureRecorded @event)
    {
        var touched = @event.Transactions.SelectMany(transaction => transaction.Tables).Select(change => (change.Schema, change.Table)).Distinct().ToList();
        var counts = tables.ToDictionary(table => (table.Schema, table.Table), table => table.Captures);
        foreach (var key in touched)
        {
            counts[key] = counts.GetValueOrDefault(key) + 1;
        }

        return [.. counts.Select(count => new TableTouched(count.Key.Schema, count.Key.Table, count.Value))];
    }

    static List<MinuteBucket> AddToMinute(IEnumerable<MinuteBucket> buckets, DateTimeOffset occurred)
    {
        var utc = occurred.UtcDateTime;
        var minute = new DateTimeOffset(utc.Year, utc.Month, utc.Day, utc.Hour, utc.Minute, 0, TimeSpan.Zero);
        var list = buckets.ToList();
        var index = list.FindIndex(bucket => bucket.Minute == minute);
        if (index >= 0)
        {
            list[index] = list[index] with { Captures = list[index].Captures + 1 };
            return list;
        }

        list.Add(new MinuteBucket(minute, 1));
        return [.. list.OrderBy(bucket => bucket.Minute).TakeLast(MinutesKept)];
    }

    static IEnumerable<ScenarioCaptures> AddToScenario(IEnumerable<ScenarioCaptures> scenarios, ScenarioName scenario)
    {
        if (scenario == ScenarioName.None)
        {
            return scenarios;
        }

        var list = scenarios.ToList();
        var index = list.FindIndex(entry => entry.Scenario == scenario);
        if (index >= 0)
        {
            list[index] = list[index] with { Captures = list[index].Captures + 1 };
        }
        else
        {
            list.Add(new ScenarioCaptures(scenario, 1));
        }

        return list;
    }
}
