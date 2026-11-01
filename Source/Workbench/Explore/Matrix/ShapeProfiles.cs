// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Captures;
using Cratis.Prologue.Workbench.Captures.Live;
using Cratis.Prologue.Workbench.Captures.Recording;
using Cratis.Prologue.Workbench.Explore.Observations;
using Cratis.Prologue.Workbench.Scenarios;

namespace Cratis.Prologue.Workbench.Explore.Matrix;

/// <summary>
/// Folds a capture into the profile of its command shape.
/// </summary>
public static class ShapeProfiles
{
    /// <summary>
    /// The most handler spans kept per shape.
    /// </summary>
    public const int HandlersKept = 5;

    /// <summary>
    /// Folds a capture that holds a command into the profile of its shape.
    /// </summary>
    /// <param name="current">The profile so far, or null when the shape is new.</param>
    /// <param name="event">The capture.</param>
    /// <returns>The new profile, with its observations worked out again.</returns>
    public static ShapeProfile Fold(ShapeProfile? current, CaptureRecorded @event)
    {
        var profile = current ?? new(@event.Http.Shape, 0, 0, [], [], [], [], []);
        var changes = @event.Transactions.SelectMany(transaction => transaction.Tables).ToList();
        var withoutWrites = changes.Count == 0 ? 1 : 0;

        var folded = profile with
        {
            Captures = profile.Captures + 1,
            WithoutWrites = profile.WithoutWrites + withoutWrites,
            Statuses = FoldStatus(profile.Statuses, @event.Http.Status, changes),
            Writes = TableWrites.Add(profile.Writes, changes),
            Handlers = FoldHandler(profile.Handlers, @event.Spans),
            Scenarios = FoldScenario(profile.Scenarios, @event.Scenario)
        };

        return folded with { Observations = [.. ShapeObservations.Of(folded)] };
    }

    static IEnumerable<StatusProfile> FoldStatus(IEnumerable<StatusProfile> statuses, HttpStatus status, List<RecordedTableChange> changes)
    {
        var list = statuses.ToList();
        var index = list.FindIndex(entry => entry.Status == status);
        var existing = index < 0 ? new StatusProfile(status, 0, 0, []) : list[index];
        var updated = existing with
        {
            Captures = existing.Captures + 1,
            WithoutWrites = existing.WithoutWrites + (changes.Count == 0 ? 1 : 0),
            Writes = TableWrites.Add(existing.Writes, changes)
        };

        if (index < 0)
        {
            list.Add(updated);
        }
        else
        {
            list[index] = updated;
        }

        return [.. list.OrderByDescending(entry => entry.Captures)];
    }

    static IEnumerable<HandlerCount> FoldHandler(IEnumerable<HandlerCount> handlers, IEnumerable<RecordedSpan> spans)
    {
        var first = spans.OrderBy(span => span.OffsetMilliseconds).FirstOrDefault();
        if (first is null)
        {
            return handlers;
        }

        var counts = handlers.ToDictionary(entry => entry.Span, entry => entry.Captures);
        counts[first.Name] = counts.GetValueOrDefault(first.Name) + 1;
        return [.. counts.OrderByDescending(entry => entry.Value).Take(HandlersKept).Select(entry => new HandlerCount(entry.Key, entry.Value))];
    }

    static IEnumerable<ScenarioCaptures> FoldScenario(IEnumerable<ScenarioCaptures> scenarios, ScenarioName scenario)
    {
        if (scenario == ScenarioName.None)
        {
            return scenarios;
        }

        var counts = scenarios.ToDictionary(entry => entry.Scenario, entry => entry.Captures);
        counts[scenario] = counts.GetValueOrDefault(scenario) + 1;
        return [.. counts.Select(entry => new ScenarioCaptures(entry.Key, entry.Value))];
    }
}
