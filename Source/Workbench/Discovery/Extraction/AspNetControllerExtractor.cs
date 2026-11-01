// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.RegularExpressions;

namespace Cratis.Prologue.Workbench.Discovery.Extraction;

/// <summary>
/// Reads ASP.NET controllers and minimal API endpoints without an AI agent: route attributes become command shapes, and
/// the actions that change state become capabilities named from their action names. GET actions are queries, which captures
/// never record, so they are not capabilities.
/// </summary>
public static partial class AspNetControllerExtractor
{
    static readonly Dictionary<string, string> _verbs = new(StringComparer.Ordinal)
    {
        ["HttpPost"] = "POST",
        ["HttpPut"] = "PUT",
        ["HttpDelete"] = "DELETE",
        ["HttpPatch"] = "PATCH"
    };

    [GeneratedRegex(@"^\[(?<group>[^\]]*)\]", RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 1000)]
    private static partial Regex AttributeGroup { get; }

    [GeneratedRegex(@"(?<![\w.])(?<name>\w+)\s*(?:\(\s*(?:\w+\s*=\s*""[^""]*""\s*,\s*)?""(?<template>[^""]*)""[^)]*\)|\([^)]*\))?", RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 1000)]
    private static partial Regex AttributeParts { get; }

    [GeneratedRegex(@"\bclass\s+(?<name>\w+)", RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 1000)]
    private static partial Regex ClassDeclaration { get; }

    [GeneratedRegex(@"\b(?:public|internal|protected)\b[^=;(]*?\b(?<name>\w+)\s*(?:<[^>]*>)?\s*\(", RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 1000)]
    private static partial Regex Member { get; }

    [GeneratedRegex(@"\.Map(?<verb>Post|Put|Delete|Patch)\(\s*""(?<path>[^""]*)""", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 1000)]
    private static partial Regex MinimalEndpoint { get; }

    /// <summary>
    /// Extracts the capabilities of the controllers and endpoints in a C# file.
    /// </summary>
    /// <param name="file">The file's path, which is recorded with each capability.</param>
    /// <param name="content">The file's content.</param>
    /// <returns>The capabilities found.</returns>
    public static IEnumerable<FoundCapability> Extract(SourcePath file, string content)
    {
        var found = new List<FoundCapability>();
        found.AddRange(Controllers(file, content));
        found.AddRange(MinimalEndpoints(file, content));
        return found;
    }

    static IEnumerable<FoundCapability> Controllers(SourcePath file, string content)
    {
        var attributes = new List<Attribute>();
        string? controller = null;
        string? controllerRoute = null;

        foreach (var raw in content.Split('\n'))
        {
            var line = raw.Trim();
            while (line.StartsWith('[') && AttributeGroup.Match(line) is { Success: true } group)
            {
                attributes.AddRange(Attributes(group.Groups["group"].Value));
                line = line[group.Length..].TrimStart();
            }

            if (line.Length == 0 || line.StartsWith("//", StringComparison.Ordinal))
            {
                continue;
            }

            if (ClassDeclaration.Match(line) is { Success: true } declaration)
            {
                var name = declaration.Groups["name"].Value;
                var isController = name.EndsWith("Controller", StringComparison.Ordinal) || attributes.Exists(_ => _.Name == "ApiController");
                controller = isController ? name.RemoveSuffix("Controller") : null;
                controllerRoute = attributes.FirstOrDefault(_ => _.Name == "Route")?.Template;
            }
            else if (controller is not null && attributes.Exists(_ => _verbs.ContainsKey(_.Name)) && Member.Match(line) is { Success: true } member)
            {
                foreach (var capability in Capabilities(file, controller, controllerRoute, member.Groups["name"].Value, attributes))
                {
                    yield return capability;
                }
            }

            attributes.Clear();
        }
    }

    static IEnumerable<FoundCapability> Capabilities(SourcePath file, string controller, string? controllerRoute, string action, List<Attribute> attributes)
    {
        var actionRoute = attributes.FirstOrDefault(_ => _.Name == "Route")?.Template;
        var shapes = attributes
            .Where(_ => _verbs.ContainsKey(_.Name))
            .Select(_ => RouteTemplate.Shape(_verbs[_.Name], controllerRoute, _.Template ?? actionRoute, controller, action))
            .DistinctBy(_ => _.Value)
            .ToList();

        if (shapes.Count > 0)
        {
            yield return new FoundCapability(new ModuleName(controller), new CapabilityName(Humanize.Name(action)), shapes, file);
        }
    }

    static IEnumerable<FoundCapability> MinimalEndpoints(SourcePath file, string content)
    {
        foreach (Match match in MinimalEndpoint.Matches(content))
        {
            var verb = match.Groups["verb"].Value.ToUpperInvariant();
            var path = match.Groups["path"].Value;
            var shape = RouteTemplate.Normalize(verb, path);
            var literals = shape.Value[(shape.Value.IndexOf(' ') + 1)..].Split('/', StringSplitOptions.RemoveEmptyEntries).Where(_ => _ != "{id}" && !_.Equals("api", StringComparison.OrdinalIgnoreCase)).ToList();
            var module = literals.Count > 0 ? Humanize.Name(literals[0]) : "Endpoints";
            var capability = literals.Count > 1 ? Humanize.Name(literals[^1]) : $"{VerbWord(verb)} {module.ToLowerInvariant()}";
            yield return new FoundCapability(new ModuleName(module), new CapabilityName(capability), [shape], file);
        }
    }

    static string RemoveSuffix(this string name, string suffix) => name.EndsWith(suffix, StringComparison.Ordinal) ? name[..^suffix.Length] : name;

    static string VerbWord(string verb) => verb switch
    {
        "POST" => "Create",
        "PUT" or "PATCH" => "Update",
        _ => "Delete"
    };

    static IEnumerable<Attribute> Attributes(string group) =>
        AttributeParts.Matches(group).Select(_ => new Attribute(_.Groups["name"].Value, _.Groups["template"].Success ? _.Groups["template"].Value : null));

    sealed record Attribute(string Name, string? Template);
}
