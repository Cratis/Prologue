// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Discover.Findings;

namespace Cratis.Prologue.Workbench.Discover.Recording;

/// <summary>
/// Decides what an analysis is worth recording. Finding the same thing again is not a fact: only what was added, changed or
/// is gone is appended, so analyzing again at the same commit records nothing.
/// </summary>
public static class CodeAnalysisDecision
{
    /// <summary>
    /// Decides the facts to record for an outcome.
    /// </summary>
    /// <param name="outcome">What the analysis ended in.</param>
    /// <param name="current">What is recorded already.</param>
    /// <returns>The events to append, in order; none when nothing changed.</returns>
    public static IEnumerable<object> Decide(AnalysisOutcome outcome, CodeFindings? current)
    {
        var known = current ?? NoFindings.For(new(Guid.Empty));
        var subject = outcome.Subject;
        var complete = outcome.Failure is null;
        var events = new List<object>();

        events.AddRange(Capabilities(outcome, known, complete));
        events.AddRange(DecisionPoints(outcome, known, complete));
        events.AddRange(Dependencies(outcome, known, complete));

        events.AddRange(RunEvents(outcome, known.RunOf(subject)));
        return events;
    }

    static IEnumerable<object> RunEvents(AnalysisOutcome outcome, AnalysisRun? run)
    {
        var subject = outcome.Subject;
        if (outcome.Failure is { } failure)
        {
            if (run?.Reason != failure)
            {
                yield return new CodeAnalysisFailed(subject, outcome.Name, failure);
            }
        }
        else if (IsNewRun(run, outcome))
        {
            yield return new CodeAnalysisCompleted(subject, outcome.Name, outcome.Source, outcome.Files);
        }
    }

    static bool IsNewRun(AnalysisRun? run, AnalysisOutcome outcome) => run is null || run.Reason is not null || run.Source != outcome.Source || run.Files != outcome.Files;

    static IEnumerable<object> Capabilities(AnalysisOutcome outcome, CodeFindings known, bool complete)
    {
        // A capability is its module and the routes it serves (or, with none, the file): the name is the model's wording and may change
        // from one analysis of unchanged code to the next.
        var subject = outcome.Subject;
        var recorded = known.Capabilities.Where(_ => _.Subject == subject).ToList();
        var found = outcome.Findings.Capabilities.GroupBy(_ => FindingKeys.Capability(_.Module, _.Capability)).Select(_ => _.First()).ToList();
        var pairs = FindingPairs.Of(
            found,
            recorded,
            (item, before) => FindingKeys.Capability(item.Module, item.Capability) == FindingKeys.Capability(before.Module, before.Capability),
            (item, before) => SameModule(item.Module, before.Module) && item.Shapes.Any() && FindingKeys.SameShapes(item.Shapes, before.Shapes),
            (item, before) => SameModule(item.Module, before.Module) && !item.Shapes.Any() && !before.Shapes.Any() && SameFile(item.File, before.File));

        return Events(
            pairs,
            pair => !(SameFile(pair.Found.File, pair.Recorded.File) && FindingKeys.SameShapes(pair.Found.Shapes, pair.Recorded.Shapes)),
            pair => CapabilityChanged(subject, pair.Found, pair.Recorded),
            item => [AsMapped(subject, item)],
            gone => new CapabilityRemoved(subject, gone.Module, gone.Capability),
            complete);
    }

    static IEnumerable<object> CapabilityChanged(AnalysisSubjectId subject, FoundCapability item, MappedCapability before)
    {
        yield return AsMapped(subject, item);
        if (FindingKeys.Capability(item.Module, item.Capability) != FindingKeys.Capability(before.Module, before.Capability))
        {
            yield return new CapabilityRemoved(subject, before.Module, before.Capability);
        }
    }

    static CapabilityMapped AsMapped(AnalysisSubjectId subject, FoundCapability item) => new(subject, item.Module, item.Capability, [.. item.Shapes], item.File);

    static IEnumerable<object> DecisionPoints(AnalysisOutcome outcome, CodeFindings known, bool complete)
    {
        // A decision point is the place in the code: its file and line. The rule text is the model's wording of it.
        var subject = outcome.Subject;
        var recorded = known.DecisionPoints.Where(_ => _.Subject == subject).ToList();
        var found = outcome.Findings.DecisionPoints.GroupBy(_ => FindingKeys.DecisionPoint(_.Rule, _.File)).Select(_ => _.First()).ToList();
        var pairs = FindingPairs.Of(
            found,
            recorded,
            (item, before) => FindingKeys.DecisionPoint(item.Rule, item.File) == FindingKeys.DecisionPoint(before.Rule, before.File),
            (item, before) => SameFile(item.File, before.File) && item.Line == before.Line);

        return Events(
            pairs,
            pair => !SameDecisionPoint(pair.Found, pair.Recorded),
            pair => DecisionPointChanged(subject, pair.Found, pair.Recorded),
            item => [AsFound(subject, item)],
            gone => new DecisionPointRemoved(subject, gone.Rule, gone.File),
            complete);
    }

    static IEnumerable<object> DecisionPointChanged(AnalysisSubjectId subject, FoundDecisionPoint item, MappedDecisionPoint before)
    {
        yield return AsFound(subject, item);
        if (FindingKeys.DecisionPoint(item.Rule, item.File) != FindingKeys.DecisionPoint(before.Rule, before.File))
        {
            yield return new DecisionPointRemoved(subject, before.Rule, before.File);
        }
    }

    static DecisionPointFound AsFound(AnalysisSubjectId subject, FoundDecisionPoint item) => new(subject, item.Rule, item.File, item.Line, item.Shape, item.Kind);

    static IEnumerable<object> Dependencies(AnalysisOutcome outcome, CodeFindings known, bool complete)
    {
        // A dependency is what is depended on and how. Its detail is wording: it is only recorded when none is yet.
        var subject = outcome.Subject;
        var recorded = known.Dependencies.Where(_ => _.Subject == subject).ToList();
        var found = outcome.Findings.Dependencies.GroupBy(_ => FindingKeys.Dependency(_.Target, _.Kind)).Select(_ => _.First()).ToList();
        var pairs = FindingPairs.Of(
            found,
            recorded,
            (item, before) => FindingKeys.Dependency(item.Target, item.Kind) == FindingKeys.Dependency(before.Target, before.Kind));

        return Events(
            pairs,
            pair => string.IsNullOrWhiteSpace(pair.Recorded.Detail.Value) && !string.IsNullOrWhiteSpace(pair.Found.Detail.Value),
            pair => [AsFound(subject, pair.Found)],
            item => [AsFound(subject, item)],
            gone => new DependencyRemoved(subject, gone.Target, gone.Kind),
            complete);
    }

    static DependencyFound AsFound(AnalysisSubjectId subject, FoundDependency item) => new(subject, item.Target, item.Kind, item.Detail);

    // The changed pairs first, then what is new, then (only when the analysis was complete) what is gone.
    static IEnumerable<object> Events<TFound, TRecorded>(
        FindingPairs<TFound, TRecorded> pairs,
        Func<(TFound Found, TRecorded Recorded), bool> isChanged,
        Func<(TFound Found, TRecorded Recorded), IEnumerable<object>> changed,
        Func<TFound, IEnumerable<object>> added,
        Func<TRecorded, object> removed,
        bool complete)
    {
        foreach (var pair in pairs.Pairs.Where(isChanged))
        {
            foreach (var @event in changed(pair))
            {
                yield return @event;
            }
        }

        foreach (var item in pairs.New)
        {
            foreach (var @event in added(item))
            {
                yield return @event;
            }
        }

        if (complete)
        {
            foreach (var gone in pairs.Gone)
            {
                yield return removed(gone);
            }
        }
    }

    static bool SameModule(ModuleName left, ModuleName right) => FindingKeys.Normalize(left.Value) == FindingKeys.Normalize(right.Value);

    static bool SameFile(SourcePath left, SourcePath right) => string.Equals(left.Value, right.Value, StringComparison.OrdinalIgnoreCase);

    static bool SameDecisionPoint(FoundDecisionPoint item, MappedDecisionPoint before) =>
        item.Line == before.Line && string.Equals(item.Shape.Value, before.Shape.Value, StringComparison.OrdinalIgnoreCase) && item.Kind == before.Kind;
}
