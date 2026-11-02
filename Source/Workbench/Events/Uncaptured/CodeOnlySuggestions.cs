// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Decisions;
using Cratis.Prologue.Workbench.Discover;
using Cratis.Prologue.Workbench.Discover.Findings;
using Cratis.Prologue.Workbench.Events.Interpreting;
using Cratis.Prologue.Workbench.Events.Suggestions;
using DiscoverCapabilityName = Cratis.Prologue.Workbench.Discover.CapabilityName;

namespace Cratis.Prologue.Workbench.Events.Uncaptured;

/// <summary>
/// Suggests events for the capabilities Discover mapped in the code whose commands nobody captured. The evidence is the place in the code and what
/// it decides, so the confidence is low and the suggestion says it was found only in code.
/// </summary>
public static class CodeOnlySuggestions
{
    /// <summary>
    /// The note every code-only suggestion carries.
    /// </summary>
    public const string CodeOnlyNote = "Found only in code; no capture backs this yet.";

    /// <summary>
    /// Suggests the events.
    /// </summary>
    /// <param name="facts">What is known about the prologue.</param>
    /// <param name="taken">The keys of the suggestions the captures already gave; a capability whose event they name is evidenced by them.</param>
    /// <param name="jobFiles">The files of the jobs that explain writes with no command; a capability there is that job, which has its own suggestion.</param>
    /// <returns>The suggestions, each with the capability it came from.</returns>
    public static IReadOnlyList<(SuggestedEvent Suggestion, MappedCapability Capability)> From(EvidenceFacts facts, IReadOnlySet<SuggestionKey> taken, IReadOnlySet<string> jobFiles)
    {
        var captured = facts.Shapes.Select(profile => profile.Shape).Concat(facts.CapturedShapes).ToList();
        var found = new Dictionary<SuggestionKey, (SuggestedEvent, MappedCapability)>();
        foreach (var capability in facts.Capabilities.OrderBy(capability => capability.Module.Value, StringComparer.OrdinalIgnoreCase).ThenBy(capability => capability.Capability.Value, StringComparer.OrdinalIgnoreCase))
        {
            var shapes = capability.Shapes.Where(shape => shape.Value.Length > 0).ToList();
            if (shapes.Exists(shape => captured.Exists(seen => SuggestionNotes.Same(seen, new Captures.CommandShape(shape.Value)))) || (shapes.Count == 0 && jobFiles.Contains(capability.File.Value.ToLowerInvariant())))
            {
                continue;
            }

            var mechanical = shapes.Count == 0 ? CodeOnlyNames.FromCapability(capability.Capability.Value) : shapes.Select(shape => CodeOnlyNames.From(shape.Value)).FirstOrDefault(name => name.Length > 0) ?? string.Empty;
            if (mechanical.Length == 0)
            {
                continue;
            }

            var module = Interpretation.Naming.Pascalize(capability.Module.Value);
            var key = CaptureAttribution.KeyOf(module.Length == 0 ? "Code" : module, mechanical);
            if (taken.Contains(key))
            {
                continue;
            }

            var suggestion = Suggest(key, mechanical, module, capability, shapes, facts);
            if (found.TryGetValue(key, out var existing))
            {
                var also = new SuggestionNote(NoteKind.Observation, $"The same event also follows from {capability.Capability.Value} ({capability.File.Value}).");
                found[key] = (SuggestionCombining.Combine(existing.Item1, suggestion with { Notes = [.. suggestion.Notes, also] }), existing.Item2);
                continue;
            }

            found[key] = (suggestion, capability);
        }

        return [.. found.Values];
    }

    static SuggestedEvent Suggest(SuggestionKey key, string mechanical, string module, MappedCapability capability, List<RouteShape> shapes, EvidenceFacts facts)
    {
        var points = facts.CodePoints.Where(point => shapes.Exists(shape => SuggestionNotes.Same(point.Shape, new Captures.CommandShape(shape.Value)))).ToList();
        var code = points.Select(point => new CodeEvidence(point.File, point.Line, point.Rule.Value)).Distinct().ToList();
        if (code.Count == 0)
        {
            code.Add(new CodeEvidence(new SourceFilePath(capability.File.Value), Decisions.LineNumber.None, capability.Capability.Value));
        }

        List<SuggestionNote> notes =
        [
            new(NoteKind.Observation, CodeOnlyNote),
            .. points
                .Where(point => point.Outcome == DecisionOutcome.Refused)
                .Select(point => new SuggestionNote(NoteKind.Rule, $"Code refuses: {point.Rule.Value} ({point.File.Value}:{point.Line.Value}). Suggested rule on the command."))
        ];
        return new SuggestedEvent(
            key,
            mechanical,
            mechanical,
            module.Length == 0 ? "Code" : module,
            TableEvents.StreamOf(mechanical),
            [],
            SuggestionConfidence.ForCodeOnly(points.Count),
            new SuggestionEvidence([.. shapes.Select(shape => new ShapeEvidence(new Captures.CommandShape(shape.Value), 0))], [], code, []),
            notes,
            SuggestionOrigin.CodeOnly);
    }
}
