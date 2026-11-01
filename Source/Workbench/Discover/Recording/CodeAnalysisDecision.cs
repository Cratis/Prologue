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

        var run = known.RunOf(subject);
        if (outcome.Failure is { } failure)
        {
            if (run?.Reason != failure)
            {
                events.Add(new CodeAnalysisFailed(subject, outcome.Name, failure));
            }
        }
        else if (run is null || run.Reason is not null || run.Source != outcome.Source || run.Files != outcome.Files)
        {
            events.Add(new CodeAnalysisCompleted(subject, outcome.Name, outcome.Source, outcome.Files));
        }

        return events;
    }

    static IEnumerable<object> Capabilities(AnalysisOutcome outcome, CodeFindings known, bool complete)
    {
        // A capability is its module and the routes it serves (or, with none, the file): the name is the model's wording and may change
        // from one analysis of unchanged code to the next.
        var recorded = known.Capabilities.Where(_ => _.Subject == outcome.Subject).ToList();
        var found = outcome.Findings.Capabilities.GroupBy(_ => FindingKeys.Capability(_.Module, _.Capability)).Select(_ => _.First()).ToList();
        var pairs = FindingPairs.Of(
            found,
            recorded,
            (item, before) => FindingKeys.Capability(item.Module, item.Capability) == FindingKeys.Capability(before.Module, before.Capability),
            (item, before) => SameModule(item.Module, before.Module) && item.Shapes.Any() && FindingKeys.SameShapes(item.Shapes, before.Shapes),
            (item, before) => SameModule(item.Module, before.Module) && !item.Shapes.Any() && !before.Shapes.Any() && SameFile(item.File, before.File));

        foreach (var (item, before) in pairs.Pairs.Where(pair => !(SameFile(pair.Found.File, pair.Recorded.File) && FindingKeys.SameShapes(pair.Found.Shapes, pair.Recorded.Shapes))))
        {
            yield return new CapabilityMapped(outcome.Subject, item.Module, item.Capability, [.. item.Shapes], item.File);
            if (FindingKeys.Capability(item.Module, item.Capability) != FindingKeys.Capability(before.Module, before.Capability))
            {
                yield return new CapabilityRemoved(outcome.Subject, before.Module, before.Capability);
            }
        }

        foreach (var item in pairs.New)
        {
            yield return new CapabilityMapped(outcome.Subject, item.Module, item.Capability, [.. item.Shapes], item.File);
        }

        if (complete)
        {
            foreach (var gone in pairs.Gone)
            {
                yield return new CapabilityRemoved(outcome.Subject, gone.Module, gone.Capability);
            }
        }
    }

    static IEnumerable<object> DecisionPoints(AnalysisOutcome outcome, CodeFindings known, bool complete)
    {
        // A decision point is the place in the code: its file and line. The rule text is the model's wording of it.
        var recorded = known.DecisionPoints.Where(_ => _.Subject == outcome.Subject).ToList();
        var found = outcome.Findings.DecisionPoints.GroupBy(_ => FindingKeys.DecisionPoint(_.Rule, _.File)).Select(_ => _.First()).ToList();
        var pairs = FindingPairs.Of(
            found,
            recorded,
            (item, before) => FindingKeys.DecisionPoint(item.Rule, item.File) == FindingKeys.DecisionPoint(before.Rule, before.File),
            (item, before) => SameFile(item.File, before.File) && item.Line == before.Line);

        foreach (var (item, before) in pairs.Pairs.Where(pair => !SameDecisionPoint(pair.Found, pair.Recorded)))
        {
            yield return new DecisionPointFound(outcome.Subject, item.Rule, item.File, item.Line, item.Shape, item.Kind);
            if (FindingKeys.DecisionPoint(item.Rule, item.File) != FindingKeys.DecisionPoint(before.Rule, before.File))
            {
                yield return new DecisionPointRemoved(outcome.Subject, before.Rule, before.File);
            }
        }

        foreach (var item in pairs.New)
        {
            yield return new DecisionPointFound(outcome.Subject, item.Rule, item.File, item.Line, item.Shape, item.Kind);
        }

        if (complete)
        {
            foreach (var gone in pairs.Gone)
            {
                yield return new DecisionPointRemoved(outcome.Subject, gone.Rule, gone.File);
            }
        }
    }

    static IEnumerable<object> Dependencies(AnalysisOutcome outcome, CodeFindings known, bool complete)
    {
        // A dependency is what is depended on and how. Its detail is wording: it is only recorded when none is yet.
        var recorded = known.Dependencies.Where(_ => _.Subject == outcome.Subject).ToList();
        var found = outcome.Findings.Dependencies.GroupBy(_ => FindingKeys.Dependency(_.Target, _.Kind)).Select(_ => _.First()).ToList();
        var pairs = FindingPairs.Of(
            found,
            recorded,
            (item, before) => FindingKeys.Dependency(item.Target, item.Kind) == FindingKeys.Dependency(before.Target, before.Kind));

        foreach (var (item, _) in pairs.Pairs.Where(pair => string.IsNullOrWhiteSpace(pair.Recorded.Detail.Value) && !string.IsNullOrWhiteSpace(pair.Found.Detail.Value)))
        {
            yield return new DependencyFound(outcome.Subject, item.Target, item.Kind, item.Detail);
        }

        foreach (var item in pairs.New)
        {
            yield return new DependencyFound(outcome.Subject, item.Target, item.Kind, item.Detail);
        }

        if (complete)
        {
            foreach (var gone in pairs.Gone)
            {
                yield return new DependencyRemoved(outcome.Subject, gone.Target, gone.Kind);
            }
        }
    }

    static bool SameModule(ModuleName left, ModuleName right) => FindingKeys.Normalize(left.Value) == FindingKeys.Normalize(right.Value);

    static bool SameFile(SourcePath left, SourcePath right) => string.Equals(left.Value, right.Value, StringComparison.OrdinalIgnoreCase);

    static bool SameDecisionPoint(FoundDecisionPoint item, MappedDecisionPoint before) =>
        item.Line == before.Line && string.Equals(item.Shape.Value, before.Shape.Value, StringComparison.OrdinalIgnoreCase) && item.Kind == before.Kind;
}
