// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Discover.Findings;
using Cratis.Prologue.Workbench.Discover.Runtime;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Discover.Map;

/// <summary>
/// Builds the decision points of a prologue as the page shows them.
/// </summary>
public static class DecisionPointViews
{
    /// <summary>
    /// Matches the decision points in the findings to the runtime evidence.
    /// </summary>
    /// <param name="findings">What the analyses found.</param>
    /// <param name="definition">The prologue, which names the systems.</param>
    /// <param name="evidence">What was captured.</param>
    /// <returns>The decision points, ordered by file and line.</returns>
    public static IEnumerable<CodeDecisionPoint> Build(CodeFindings findings, PrologueDefinition? definition, RuntimeEvidence evidence) =>
        [.. findings.DecisionPoints
            .OrderBy(_ => _.File.Value, StringComparer.OrdinalIgnoreCase)
            .ThenBy(_ => _.Line.Value)
            .Select(point =>
            {
                var match = RuntimeMatching.Match(point.Kind, point.Shape, evidence);
                return new CodeDecisionPoint(point.Subject, SystemNames.Of(point.Subject, definition, findings), point.Rule, point.File, point.Line, point.Shape, point.Kind, match.Happened, match.Status, match.Runtime);
            })];
}
