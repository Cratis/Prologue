// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Discover.Findings;

namespace Cratis.Prologue.Workbench.Discover.Agents;

/// <summary>
/// Combines what the deterministic extractor and the agents found. The extractor's names win, so the map stays steady between
/// runs; the agents add what it cannot see and say more about what it found.
/// </summary>
public static class FindingsMerge
{
    /// <summary>
    /// Merges findings, dropping repeats.
    /// </summary>
    /// <param name="parts">The findings to merge, the most trusted first.</param>
    /// <returns>The merged findings.</returns>
    public static AnalysisFindings Merge(IEnumerable<AnalysisFindings> parts)
    {
        var all = parts.ToList();
        return new(
            MergeCapabilities(all.SelectMany(_ => _.Capabilities)),
            [.. all.SelectMany(_ => _.DecisionPoints).GroupBy(_ => FindingKeys.DecisionPoint(_.Rule, _.File)).Select(_ => _.First())],
            [.. all.SelectMany(_ => _.Dependencies).GroupBy(_ => FindingKeys.Dependency(_.Target, _.Kind)).Select(_ => _.First())]);
    }

    static List<FoundCapability> MergeCapabilities(IEnumerable<FoundCapability> capabilities)
    {
        var merged = new List<FoundCapability>();
        foreach (var capability in capabilities)
        {
            var key = FindingKeys.Capability(capability.Module, capability.Capability);
            var existing = merged.FindIndex(_ => FindingKeys.Capability(_.Module, _.Capability) == key || SharesShape(_, capability));
            if (existing < 0)
            {
                merged.Add(capability);
            }
            else
            {
                var before = merged[existing];
                merged[existing] = before with { Shapes = [.. before.Shapes.Concat(capability.Shapes).DistinctBy(_ => _.Value.ToLowerInvariant())] };
            }
        }

        return merged;
    }

    static bool SharesShape(FoundCapability left, FoundCapability right) =>
        left.Shapes.Any(shape => right.Shapes.Any(other => string.Equals(shape.Value, other.Value, StringComparison.OrdinalIgnoreCase)));
}
