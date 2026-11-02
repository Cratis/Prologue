// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;
using Cratis.Prologue.Workbench.Decisions;
using Cratis.Prologue.Workbench.Events.Uncaptured;

namespace Cratis.Prologue.Workbench.Events.Interpreting;

/// <summary>
/// Writes the evidence beyond the captures as text for the language model: the scenario labels, what the captures decided and the rules the code holds.
/// Every part is bounded, so a large system never blows the prompt.
/// </summary>
public static class EvidenceText
{
    const int Limit = 40;

    /// <summary>
    /// Writes the evidence.
    /// </summary>
    /// <param name="facts">What is known about the prologue.</param>
    /// <returns>The evidence; empty when there is none.</returns>
    public static string For(EvidenceFacts facts)
    {
        var builder = new StringBuilder();
        foreach (var profile in facts.Shapes.Where(profile => profile.Scenarios.Any(scenario => scenario.Scenario.Value.Length > 0)).Take(Limit))
        {
            var scenarios = string.Join(", ", profile.Scenarios.Where(scenario => scenario.Scenario.Value.Length > 0).Select(scenario => $"{scenario.Scenario.Value} ({scenario.Captures})"));
            builder.AppendLine($"- Scenario labels of {profile.Shape.Value}: {scenarios}");
        }

        foreach (var shape in facts.Decisions.GroupBy(decision => decision.Shape).Take(Limit))
        {
            var refused = shape.Where(decision => decision.Outcome == DecisionOutcome.Refused).ToList();
            var accepted = shape.Count(decision => decision.Outcome == DecisionOutcome.Accepted);
            var reasons = string.Join("; ", shape.Select(decision => decision.Reason.Text.Value).Where(reason => reason.Length > 0).Distinct().Take(3));
            builder.AppendLine($"- Decisions of {shape.Key.Value}: {accepted} accepted, {refused.Count} refused{(refused.Count > 0 ? $" ({string.Join(", ", refused.Select(decision => decision.Status.Value).Distinct().Order())})" : string.Empty)}{(reasons.Length > 0 ? $". {reasons}" : string.Empty)}");
        }

        foreach (var point in facts.CodePoints.Take(Limit))
        {
            builder.AppendLine($"- Code {(point.Outcome == DecisionOutcome.Refused ? "refuses" : "accepts")} on {point.Shape.Value}: {point.Rule.Value} ({point.File.Value}:{point.Line.Value})");
        }

        foreach (var table in facts.Uncorrelated.Take(Limit))
        {
            builder.AppendLine($"- {table.Write.Table.Value} is written by {table.Transactions} transactions with no command{UncorrelatedQuestions.When(table)} (UTC); a job in the code may explain it");
        }

        foreach (var job in facts.Jobs.Take(Limit))
        {
            builder.AppendLine($"- Code runs a job on its own: {job.Name}{(job.Detail.Length > 0 ? $" ({job.Detail})" : string.Empty)}");
        }

        return builder.ToString().TrimEnd();
    }
}
