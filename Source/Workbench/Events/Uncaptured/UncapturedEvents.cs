// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Interpretation;
using Cratis.Prologue.Interpreter.Contracts;
using Cratis.Prologue.Workbench.Discover;
using Cratis.Prologue.Workbench.Events.Interpreting;
using Cratis.Prologue.Workbench.Events.Questions;
using Cratis.Prologue.Workbench.Events.Suggestions;

namespace Cratis.Prologue.Workbench.Events.Uncaptured;

/// <summary>
/// Adds to an interpretation what the captured commands could not give it: events found only in the code, and events behind writes with no command.
/// They join the suggestions and the structure, so they go through the same review as every other suggestion.
/// </summary>
public static class UncapturedEvents
{
    /// <summary>
    /// Extends an interpretation.
    /// </summary>
    /// <param name="suggestions">The suggestions the captured commands gave.</param>
    /// <param name="model">The structure around them.</param>
    /// <param name="facts">What else is known about the prologue.</param>
    /// <param name="answers">The answers people already gave.</param>
    /// <returns>The suggestions and the structure with the uncaptured events added, and the questions the Workbench raises.</returns>
    public static (IReadOnlyList<SuggestedEvent> Suggestions, InterpretedModel Model, IReadOnlyList<RaisedQuestion> Questions) Extend(
        IReadOnlyList<SuggestedEvent> suggestions,
        InterpretedModel model,
        EvidenceFacts facts,
        IReadOnlyList<QuestionAnswer> answers)
    {
        var taken = suggestions.Select(suggestion => suggestion.Key).ToHashSet();
        var (noCommand, questions) = NoCommandSuggestions.From(facts, answers);
        noCommand = [.. noCommand.Where(item => !taken.Contains(item.Suggestion.Key))];
        var jobFiles = noCommand.Where(item => item.Job?.File.Value.Length > 0).Select(item => item.Job!.File.Value.ToLowerInvariant()).ToHashSet();
        var codeOnly = CodeOnlySuggestions.From(facts, taken, jobFiles);

        if (codeOnly.Count == 0 && noCommand.Count == 0)
        {
            return (suggestions, model, questions);
        }

        var slices = new List<ModelSlice>(model.Slices);
        slices.AddRange(codeOnly.Select(item => CodeOnlySlice(item.Suggestion, item.Capability)));
        slices.AddRange(noCommand.Select(item => NoCommandSlice(item.Suggestion, item.Job)));
        return ([.. suggestions, .. codeOnly.Select(item => item.Suggestion), .. noCommand.Select(item => item.Suggestion)], model with { Slices = slices }, questions);
    }

    static ModelSlice CodeOnlySlice(SuggestedEvent suggestion, Discover.Findings.MappedCapability capability)
    {
        var routed = capability.Shapes.Any();
        var name = Naming.Pascalize(capability.Capability.Value);
        return new ModelSlice(
            suggestion.Module,
            capability.Capability.Value,
            name,
            routed ? ExtractedSliceType.StateChange : ExtractedSliceType.Automation,
            CodeOnlySuggestions.CodeOnlyNote,
            routed ? [new ModelCommand(name, [], [], CodeOnlySuggestions.CodeOnlyNote)] : [],
            [suggestion.Key],
            [],
            [],
            []);
    }

    static ModelSlice NoCommandSlice(SuggestedEvent suggestion, CodeJob? job) =>
        new(
            suggestion.Module,
            job is null ? "Background" : job.Name,
            suggestion.Refined.Value,
            ExtractedSliceType.Automation,
            "Writes that arrive with no command.",
            [],
            [suggestion.Key],
            [],
            [],
            []);
}
