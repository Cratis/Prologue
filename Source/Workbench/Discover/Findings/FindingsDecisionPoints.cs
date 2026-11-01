// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Captures;
using Cratis.Prologue.Workbench.Decisions;

namespace Cratis.Prologue.Workbench.Discover.Findings;

/// <summary>
/// Turns what the code analyses found into the decision points the Capture page explains decisions with.
/// </summary>
public static class FindingsDecisionPoints
{
    /// <summary>
    /// Lists the decision points of the findings that a captured command can match. A refusal explains a refused command and a
    /// state change an accepted one; a schedule, and anything the code does not tie to a command, explains no command. The
    /// capability is the one whose command shapes include the decision's shape.
    /// </summary>
    /// <param name="findings">What the analyses found for a prologue.</param>
    /// <returns>The decision points, ordered by file and line.</returns>
    public static IReadOnlyList<DecisionPoint> From(CodeFindings findings) =>
        [
            .. findings.DecisionPoints
                .Where(point => point.Kind != DecisionKind.Schedule && point.Shape.Value.Length > 0)
                .OrderBy(point => point.File.Value, StringComparer.OrdinalIgnoreCase)
                .ThenBy(point => point.Line.Value)
                .Select(point => new DecisionPoint(
                    new CommandShape(point.Shape.Value),
                    point.Kind == DecisionKind.Refusal ? DecisionOutcome.Refused : DecisionOutcome.Accepted,
                    new Decisions.RuleText(point.Rule.Value),
                    new SourceFilePath(point.File.Value),
                    new Decisions.LineNumber(point.Line.Value),
                    CapabilityOf(findings, point.Shape)))
        ];

    static Decisions.CapabilityName CapabilityOf(CodeFindings findings, RouteShape shape)
    {
        var capability = findings.Capabilities.FirstOrDefault(
            mapped => mapped.Shapes.Any(candidate => string.Equals(candidate.Value, shape.Value, StringComparison.OrdinalIgnoreCase)));
        return capability is null ? Decisions.CapabilityName.None : new Decisions.CapabilityName(capability.Capability.Value);
    }
}
