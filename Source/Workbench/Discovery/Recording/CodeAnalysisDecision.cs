// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Discovery.Findings;

namespace Cratis.Prologue.Workbench.Discovery.Recording;

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
        var known = current ?? CodeFindings.Empty(new(Guid.Empty));
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
        var existing = known.Capabilities.Where(_ => _.Subject == outcome.Subject).ToDictionary(_ => FindingKeys.Capability(_.Module, _.Capability));
        var found = outcome.Findings.Capabilities.GroupBy(_ => FindingKeys.Capability(_.Module, _.Capability)).Select(_ => _.First()).ToList();

        foreach (var item in found)
        {
            var same = existing.TryGetValue(FindingKeys.Capability(item.Module, item.Capability), out var before)
                && string.Equals(before.File.Value, item.File.Value, StringComparison.OrdinalIgnoreCase) && FindingKeys.SameShapes(before.Shapes, item.Shapes);
            if (!same)
            {
                yield return new CapabilityMapped(outcome.Subject, item.Module, item.Capability, [.. item.Shapes], item.File);
            }
        }

        if (!complete)
        {
            yield break;
        }

        var kept = found.Select(_ => FindingKeys.Capability(_.Module, _.Capability)).ToHashSet();
        foreach (var gone in existing.Where(_ => !kept.Contains(_.Key)).Select(_ => _.Value))
        {
            yield return new CapabilityRemoved(outcome.Subject, gone.Module, gone.Capability);
        }
    }

    static IEnumerable<object> DecisionPoints(AnalysisOutcome outcome, CodeFindings known, bool complete)
    {
        var existing = known.DecisionPoints.Where(_ => _.Subject == outcome.Subject).ToDictionary(_ => FindingKeys.DecisionPoint(_.Rule, _.File));
        var found = outcome.Findings.DecisionPoints.GroupBy(_ => FindingKeys.DecisionPoint(_.Rule, _.File)).Select(_ => _.First()).ToList();

        foreach (var item in found)
        {
            var same = existing.TryGetValue(FindingKeys.DecisionPoint(item.Rule, item.File), out var before)
                && before.Line == item.Line && string.Equals(before.Shape.Value, item.Shape.Value, StringComparison.OrdinalIgnoreCase) && before.Kind == item.Kind;
            if (!same)
            {
                yield return new DecisionPointFound(outcome.Subject, item.Rule, item.File, item.Line, item.Shape, item.Kind);
            }
        }

        if (!complete)
        {
            yield break;
        }

        var kept = found.Select(_ => FindingKeys.DecisionPoint(_.Rule, _.File)).ToHashSet();
        foreach (var gone in existing.Where(_ => !kept.Contains(_.Key)).Select(_ => _.Value))
        {
            yield return new DecisionPointRemoved(outcome.Subject, gone.Rule, gone.File);
        }
    }

    static IEnumerable<object> Dependencies(AnalysisOutcome outcome, CodeFindings known, bool complete)
    {
        var existing = known.Dependencies.Where(_ => _.Subject == outcome.Subject).ToDictionary(_ => FindingKeys.Dependency(_.Target, _.Kind));
        var found = outcome.Findings.Dependencies.GroupBy(_ => FindingKeys.Dependency(_.Target, _.Kind)).Select(_ => _.First()).ToList();

        foreach (var item in found)
        {
            var same = existing.TryGetValue(FindingKeys.Dependency(item.Target, item.Kind), out var before) && before.Detail == item.Detail;
            if (!same)
            {
                yield return new DependencyFound(outcome.Subject, item.Target, item.Kind, item.Detail);
            }
        }

        if (!complete)
        {
            yield break;
        }

        var kept = found.Select(_ => FindingKeys.Dependency(_.Target, _.Kind)).ToHashSet();
        foreach (var gone in existing.Where(_ => !kept.Contains(_.Key)).Select(_ => _.Value))
        {
            yield return new DependencyRemoved(outcome.Subject, gone.Target, gone.Kind);
        }
    }
}
