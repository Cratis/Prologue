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
    /// The most let-go things remembered, so one that comes back is not counted as a new thing. Past this the oldest is forgotten and
    /// counted again if it returns, which keeps the document bounded.
    /// </summary>
    public const int EvictedKept = 5000;

    /// <summary>
    /// The most distinct sequences counted per kind. The rest are folded into one sequence without steps that only says how many
    /// things went through sequences that are not listed.
    /// </summary>
    public const int SequencesKept = 50;

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
    /// <param name="evicted">The things let go so far, the one let go last at the end.</param>
    /// <param name="sequences">The sequences counted so far.</param>
    /// <param name="command">The recorded command.</param>
    /// <returns>The trails, the things let go and the sequences, not described yet.</returns>
    public static (IEnumerable<EntityTrail> Trails, IEnumerable<EvictedEntity> Evicted, IEnumerable<JourneySequence> Sequences) Link(
        IEnumerable<EntityTrail> trails,
        IEnumerable<EvictedEntity> evicted,
        IEnumerable<JourneySequence> sequences,
        RecordedCommand command)
    {
        var kept = trails.ToList();
        var letGo = evicted.ToList();
        var counted = sequences.ToList();
        if (!IsStep(command))
        {
            return (kept, letGo, counted);
        }

        foreach (var identity in PathIdentities.From(command.Path).Distinct())
        {
            Follow(kept, letGo, counted, identity, command.Shape);
        }

        var dropped = kept.Take(Math.Max(0, kept.Count - TrailsKept)).Select(trail => new EvictedEntity(trail.Kind, trail.Id));
        letGo.AddRange(dropped);
        return (kept.TakeLast(TrailsKept).ToList(), letGo.TakeLast(EvictedKept).ToList(), Cap(counted));
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

    static void Follow(List<EntityTrail> kept, List<EvictedEntity> letGo, List<JourneySequence> counted, PathIdentity identity, CommandShape shape)
    {
        var index = kept.FindIndex(trail => trail.Kind == identity.Kind && trail.Id == identity.Id);
        if (index < 0 && WasLetGo(letGo, identity))
        {
            // Already counted once, in a sequence that cannot be told apart from the others now; counting it again would
            // make it a new one-step thing every time it comes back.
            return;
        }

        var before = index < 0 ? [] : kept[index].Steps.ToList();
        var after = Extend(before, shape);
        if (after.Count == before.Count)
        {
            return;
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

    static bool WasLetGo(List<EvictedEntity> letGo, PathIdentity identity) =>
        letGo.Exists(gone => gone.Kind == identity.Kind && gone.Id == identity.Id);

    static List<JourneySequence> Cap(List<JourneySequence> counted)
    {
        var capped = new List<JourneySequence>();
        foreach (var kind in counted.Select(sequence => sequence.Kind).Distinct())
        {
            var ofKind = counted.Where(sequence => sequence.Kind == kind).ToList();
            var listed = ofKind.Where(sequence => sequence.Steps.Any()).OrderByDescending(sequence => sequence.Entities).ToList();
            var others = ofKind.Where(sequence => !sequence.Steps.Any()).Sum(sequence => sequence.Entities)
                + listed.Skip(SequencesKept).Sum(sequence => sequence.Entities);
            capped.AddRange(listed.Take(SequencesKept));
            if (others > 0)
            {
                capped.Add(new JourneySequence(kind, [], others, 0, CommandShape.None));
            }
        }

        return capped;
    }

    static List<CommandShape> Extend(List<CommandShape> steps, CommandShape shape)
    {
        return IsFull(steps) || RepeatsLast(steps, shape) ? steps : [.. steps, shape];
    }

    static bool IsFull(List<CommandShape> steps) => steps.Count >= StepsKept;

    static bool RepeatsLast(List<CommandShape> steps, CommandShape shape) => steps.Count > 0 && steps[^1] == shape;

    static void Count(List<JourneySequence> counted, EntityKind kind, List<CommandShape> steps, int change)
    {
        var index = counted.FindIndex(sequence => sequence.Kind == kind && sequence.Steps.SequenceEqual(steps));
        if (index < 0 && change < 0)
        {
            // The sequence was folded into the others when the list was capped; the thing leaves them from there.
            index = counted.FindIndex(sequence => sequence.Kind == kind && !sequence.Steps.Any());
        }

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
        if (steps.Count == 0)
        {
            return CommandShape.None;
        }

        var longer = ofKind
            .Where(other => other.Entities > 0 && other.Steps.Count() > steps.Count && other.Steps.Take(steps.Count).SequenceEqual(steps))
            .OrderByDescending(other => other.Entities)
            .FirstOrDefault();
        return longer is null ? CommandShape.None : longer.Steps.ElementAt(steps.Count);
    }
}
