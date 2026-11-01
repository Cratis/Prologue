// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Captures;
using Cratis.Prologue.Workbench.Explore.Observations;

namespace Cratis.Prologue.Workbench.Explore.Journeys;

/// <summary>
/// Links captures that share an identity in their request path into the sequence of commands each thing went through, and
/// describes the sequences that are common. Everything here is pure; the read model keeps the state.
/// </summary>
public static class JourneyInference
{
    /// <summary>
    /// The most things followed at once. When more arrive the one untouched the longest is let go; the sequence it went through
    /// stays counted, it just cannot grow further.
    /// </summary>
    public const int TrailsKept = 1000;

    /// <summary>
    /// The most steps followed per thing.
    /// </summary>
    public const int StepsKept = 10;

    static readonly string[] _reads = ["GET", "HEAD", "OPTIONS"];

    /// <summary>
    /// Gets whether a command is a step in a journey: it has a shape and it is not only a read.
    /// </summary>
    /// <param name="command">The recorded command.</param>
    /// <returns>True when it can be a step.</returns>
    public static bool IsStep(RecordedCommand command) =>
        !command.Shape.IsNone && !_reads.Contains(command.Method.Value, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Adds a command to the trail of every thing its path identifies.
    /// </summary>
    /// <param name="trails">The trails followed so far, the one touched last at the end.</param>
    /// <param name="sequences">The sequences counted so far.</param>
    /// <param name="command">The recorded command.</param>
    /// <returns>The trails and the sequences, not described yet.</returns>
    public static (IEnumerable<EntityTrail> Trails, IEnumerable<JourneySequence> Sequences) Link(
        IEnumerable<EntityTrail> trails,
        IEnumerable<JourneySequence> sequences,
        RecordedCommand command)
    {
        var kept = trails.ToList();
        var counted = sequences.ToList();
        if (!IsStep(command))
        {
            return (kept, counted);
        }

        foreach (var identity in PathIdentities.From(command.Path).Distinct())
        {
            var index = kept.FindIndex(trail => trail.Kind == identity.Kind && trail.Id == identity.Id);
            var before = index < 0 ? [] : kept[index].Steps.ToList();
            var after = Extend(before, command.Shape);
            if (after.Count == before.Count)
            {
                continue;
            }

            if (index >= 0)
            {
                kept.RemoveAt(index);
            }

            kept.Add(new EntityTrail(identity.Kind, identity.Id, after));
            if (before.Count > 0)
            {
                Count(counted, identity.Kind, before, -1);
            }

            Count(counted, identity.Kind, after, 1);
        }

        return (kept.TakeLast(TrailsKept).ToList(), counted);
    }

    /// <summary>
    /// Works out the share of each sequence among the things of its kind and what the others went on to do, so the unfinished
    /// ones stand out. The most common sequence of each kind comes first.
    /// </summary>
    /// <param name="sequences">The sequences with their counts.</param>
    /// <returns>The sequences described.</returns>
    public static IEnumerable<JourneySequence> Describe(IEnumerable<JourneySequence> sequences)
    {
        var all = sequences.Where(sequence => sequence.Entities > 0).ToList();
        var described = new List<JourneySequence>();
        foreach (var kind in all.Select(sequence => sequence.Kind).Distinct())
        {
            var ofKind = all.Where(sequence => sequence.Kind == kind).ToList();
            var total = ofKind.Sum(sequence => sequence.Entities);
            described.AddRange(ofKind
                .OrderByDescending(sequence => sequence.Entities)
                .Select(sequence => sequence with { Share = Share.Percent(sequence.Entities, total), ContinuedBy = Continuation(sequence, ofKind) }));
        }

        return described;
    }

    static List<CommandShape> Extend(List<CommandShape> steps, CommandShape shape)
    {
        if (steps.Count >= StepsKept || (steps.Count > 0 && steps[^1] == shape))
        {
            return steps;
        }

        return [.. steps, shape];
    }

    static void Count(List<JourneySequence> counted, EntityKind kind, List<CommandShape> steps, int change)
    {
        var index = counted.FindIndex(sequence => sequence.Kind == kind && sequence.Steps.SequenceEqual(steps));
        if (index >= 0)
        {
            counted[index] = counted[index] with { Entities = counted[index].Entities + change };
        }
        else
        {
            counted.Add(new JourneySequence(kind, steps, change, 0, CommandShape.None));
        }
    }

    static CommandShape Continuation(JourneySequence sequence, List<JourneySequence> ofKind)
    {
        var steps = sequence.Steps.ToList();
        var longer = ofKind
            .Where(other => other.Entities > 0 && other.Steps.Count() > steps.Count && other.Steps.Take(steps.Count).SequenceEqual(steps))
            .OrderByDescending(other => other.Entities)
            .FirstOrDefault();
        return longer is null ? CommandShape.None : longer.Steps.ElementAt(steps.Count);
    }
}
