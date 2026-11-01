// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Captures.Live;

namespace Cratis.Prologue.Workbench.Decisions;

/// <summary>
/// Selects the reason for a decision.
/// </summary>
public static class DecisionReasoning
{
    /// <summary>
    /// Selects a reason. The first decision point matching the command shape and the outcome supplies the rule, its file and
    /// line. Without one, the observed pattern is the reason. The capability comes from any decision point of the shape, as
    /// the capability does not depend on the outcome.
    /// </summary>
    /// <param name="capture">The capture the decision was made about.</param>
    /// <param name="points">The decision points found in the code, none when Discover has found none.</param>
    /// <returns>The reason.</returns>
    public static DecisionReason Select(RecordedCapture capture, IEnumerable<DecisionPoint> points)
    {
        var ofShape = points.Where(point => point.Shape == capture.Http.Shape).ToList();
        var capability = ofShape.Select(point => point.Capability).FirstOrDefault() ?? CapabilityName.None;
        var matching = ofShape.FirstOrDefault(point => point.Outcome == capture.Outcome);

        return matching is null
            ? new(ReasonSource.Observed, ObservedPattern.Describe(capture), SourceFilePath.None, LineNumber.None, capability)
            : new(ReasonSource.Discovered, matching.Rule, matching.File, matching.Line, capability);
    }
}
