// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Discovery.Findings;
using Cratis.Prologue.Workbench.Discovery.Runtime;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Discovery.Map;

/// <summary>
/// Builds the application map from what the code says and what the running system did.
/// </summary>
public static class ApplicationMapBuilder
{
    /// <summary>
    /// Builds the map. A capability is seen at runtime when a captured command shape belongs to it, and is otherwise found only
    /// in code, which includes every capability no HTTP command reaches.
    /// </summary>
    /// <param name="findings">What the analyses found.</param>
    /// <param name="definition">The prologue, which orders and names the systems.</param>
    /// <param name="evidence">What was captured.</param>
    /// <returns>The map.</returns>
    public static ApplicationMap Build(CodeFindings findings, PrologueDefinition? definition, RuntimeEvidence evidence)
    {
        var order = definition?.Systems.Select(_ => AnalysisSubjectId.For(_.Id)).ToList() ?? [];
        var systems = findings.Capabilities
            .GroupBy(_ => _.Subject)
            .OrderBy(_ => order.IndexOf(_.Key) is var index and >= 0 ? index : int.MaxValue)
            .Select(group => new MapSystem(group.Key, SystemNames.Of(group.Key, definition, findings), Modules(group, evidence)))
            .ToList();

        var capabilities = systems.SelectMany(_ => _.Modules).SelectMany(_ => _.Capabilities).ToList();
        return new(
            findings.Id,
            systems,
            capabilities.Count,
            capabilities.Count(_ => _.Runtime == RuntimeStatus.Seen),
            capabilities.Count(_ => _.Runtime == RuntimeStatus.CodeOnly),
            findings.DecisionPoints.Count(),
            findings.Runs.Sum(_ => _.Files.Value));
    }

    static List<MapModule> Modules(IEnumerable<MappedCapability> capabilities, RuntimeEvidence evidence) =>
        [.. capabilities
            .GroupBy(_ => FindingKeys.Normalize(_.Module.Value))
            .Select(module => new MapModule(
                module.First().Module,
                [.. module.OrderBy(_ => _.Capability.Value, StringComparer.OrdinalIgnoreCase).Select(_ => Capability(_, evidence))]))
            .OrderBy(_ => _.Name.Value, StringComparer.OrdinalIgnoreCase)];

    static MapCapability Capability(MappedCapability capability, RuntimeEvidence evidence)
    {
        var seen = RuntimeMatching.Seen(capability.Shapes, evidence);
        return new(capability.Capability, capability.Shapes, capability.File, seen > 0 ? RuntimeStatus.Seen : RuntimeStatus.CodeOnly, seen);
    }
}
