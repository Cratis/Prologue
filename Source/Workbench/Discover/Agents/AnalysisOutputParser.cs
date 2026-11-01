// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Discover.Extraction;

namespace Cratis.Prologue.Workbench.Discover.Agents;

/// <summary>
/// Turns what the agents answered into findings. An answer is never trusted whole: an entry that names a file the agent was not
/// shown, a line outside the file or a kind that does not exist is dropped with its reason, and the rest are kept.
/// </summary>
public static class AnalysisOutputParser
{
    /// <summary>
    /// Reads the Dramaturg's answer.
    /// </summary>
    /// <param name="text">The answer.</param>
    /// <param name="files">The files the agent was shown, with their line counts.</param>
    /// <returns>The capabilities.</returns>
    public static ParsedFindings<FoundCapability> Capabilities(string text, IReadOnlyDictionary<string, int> files)
    {
        var entries = AgentJson.Entries(text, "capabilities", out var problem);
        var items = new List<FoundCapability>();
        var dropped = Problem(problem);

        foreach (var entry in entries)
        {
            var module = AgentJson.Text(entry, "module");
            var capability = AgentJson.Text(entry, "capability");
            var file = ShownFile(AgentJson.Text(entry, "file"), files);
            if (capability.Length == 0 || module.Length == 0)
            {
                dropped.Add("A capability has no module or no name");
            }
            else if (file is null)
            {
                dropped.Add($"Capability '{capability}' names a file that was not shown: '{AgentJson.Text(entry, "file")}'");
            }
            else
            {
                var shapes = AgentJson.Texts(entry, "commands").Select(RouteTemplate.Parse).OfType<RouteShape>().DistinctBy(_ => _.Value).ToList();
                items.Add(new FoundCapability(new ModuleName(module), new CapabilityName(capability), shapes, file));
            }
        }

        return new(items, dropped);
    }

    /// <summary>
    /// Reads the Prompter's answer.
    /// </summary>
    /// <param name="text">The answer.</param>
    /// <param name="files">The files the agent was shown, with their line counts.</param>
    /// <returns>The decision points.</returns>
    public static ParsedFindings<FoundDecisionPoint> DecisionPoints(string text, IReadOnlyDictionary<string, int> files)
    {
        var entries = AgentJson.Entries(text, "decisionPoints", out var problem);
        var items = new List<FoundDecisionPoint>();
        var dropped = Problem(problem);

        foreach (var entry in entries)
        {
            var rule = AgentJson.Text(entry, "rule");
            var file = ShownFile(AgentJson.Text(entry, "file"), files);
            var line = AgentJson.Number(entry, "line");
            if (rule.Length == 0)
            {
                dropped.Add("A decision point has no rule");
            }
            else if (file is null)
            {
                dropped.Add($"Decision point '{rule}' names a file that was not shown: '{AgentJson.Text(entry, "file")}'");
            }
            else if (line < 1 || line > files[file.Value])
            {
                dropped.Add($"Decision point '{rule}' is on line {line}, outside {file.Value} ({files[file.Value]} lines)");
            }
            else if (Kind(AgentJson.Text(entry, "kind")) is not { } kind)
            {
                dropped.Add($"Decision point '{rule}' has the unknown kind '{AgentJson.Text(entry, "kind")}'");
            }
            else
            {
                items.Add(new FoundDecisionPoint(new RuleText(rule), file, new LineNumber(line), RouteTemplate.Parse(AgentJson.Text(entry, "command")) ?? RouteShape.None, kind));
            }
        }

        return new(items, dropped);
    }

    /// <summary>
    /// Reads the Usher's answer.
    /// </summary>
    /// <param name="text">The answer.</param>
    /// <returns>The dependencies.</returns>
    public static ParsedFindings<FoundDependency> Dependencies(string text)
    {
        var entries = AgentJson.Entries(text, "dependencies", out var problem);
        var items = new List<FoundDependency>();
        var dropped = Problem(problem);

        foreach (var entry in entries)
        {
            var target = AgentJson.Text(entry, "target");
            var kind = AgentJson.Text(entry, "kind").ToLowerInvariant() switch
            {
                "http" => (DependencyKind?)DependencyKind.Http,
                "database" or "db" => DependencyKind.Database,
                "job" or "schedule" => DependencyKind.Job,
                "queue" or "bus" => DependencyKind.Queue,
                _ => null
            };

            if (target.Length == 0)
            {
                dropped.Add("A dependency has no target");
            }
            else if (kind is null)
            {
                dropped.Add($"Dependency '{target}' has the unknown kind '{AgentJson.Text(entry, "kind")}'");
            }
            else
            {
                items.Add(new FoundDependency(new DependencyTarget(target), kind.Value, new DependencyDetail(AgentJson.Text(entry, "detail"))));
            }
        }

        return new(items, dropped);
    }

    static List<string> Problem(string? problem) => problem is null ? [] : [problem];

    static DecisionKind? Kind(string text) => text.ToLowerInvariant().Replace("_", string.Empty, StringComparison.Ordinal) switch
    {
        "refusal" or "refuse" => DecisionKind.Refusal,
        "statechange" or "state" => DecisionKind.StateChange,
        "schedule" or "job" => DecisionKind.Schedule,
        _ => null
    };

    static SourcePath? ShownFile(string file, IReadOnlyDictionary<string, int> files)
    {
        var normalized = file.Replace('\\', '/');
        normalized = (normalized.StartsWith("./", StringComparison.Ordinal) ? normalized[2..] : normalized).TrimStart('/');
        var match = files.Keys.FirstOrDefault(_ => string.Equals(_, normalized, StringComparison.OrdinalIgnoreCase));
        return match is null ? null : new SourcePath(match);
    }
}
