// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Captures;
using Cratis.Prologue.Workbench.Discover;
using Cratis.Prologue.Workbench.Events.Boundaries;
using Cratis.Prologue.Workbench.Events.Suggestions;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Events.Accepted;

/// <summary>
/// Builds the accepted model from the review. Pure.
/// </summary>
public static class AcceptedModels
{
    /// <summary>
    /// Builds the model.
    /// </summary>
    /// <param name="prologue">The prologue.</param>
    /// <param name="review">The review; null when nothing was ever interpreted.</param>
    /// <returns>The model; empty when nothing was interpreted.</returns>
    public static AcceptedModel From(PrologueId prologue, EventReview? review)
    {
        if (review is null)
        {
            return new AcceptedModel(prologue, string.Empty, 0, 0, 0, 0, [], []);
        }

        var suggestions = review.Suggestions.ToDictionary(suggestion => suggestion.Suggestion.Key);
        bool Kept(SuggestionKey key) => suggestions.TryGetValue(key, out var found) && found.Status != SuggestionStatus.Rejected && (!found.Withdrawn || found.Status == SuggestionStatus.Accepted);

        var slices = review.Model.Slices
            .Select(slice => (Slice: slice, Built: Build(slice, suggestions, review, Kept)))
            .Where(item => item.Built is not null)
            .ToList();
        var modules = slices
            .GroupBy(item => item.Slice.Module)
            .Select(module => new AcceptedModule(
                module.Key,
                [.. module.GroupBy(item => item.Slice.Feature).Select(feature => new AcceptedFeature(feature.Key, [.. feature.Select(item => item.Built!)]))]))
            .ToList();

        var live = review.Suggestions.Where(suggestion => Kept(suggestion.Suggestion.Key)).ToList();
        var rules = slices.SelectMany(item => item.Built!.Rules).Concat(Chosen(review)).Distinct().ToList();
        return new AcceptedModel(
            prologue,
            review.Model.SystemName,
            live.Count(suggestion => suggestion.Status == SuggestionStatus.Accepted),
            live.Count(suggestion => suggestion.Status == SuggestionStatus.Suggested),
            review.Suggestions.Count(suggestion => suggestion.Status == SuggestionStatus.Rejected),
            review.Boundaries.Count(boundary => review.Choices.All(choice => choice.Key != boundary.Key)),
            modules,
            rules);
    }

    static AcceptedSlice? Build(ModelSlice slice, Dictionary<SuggestionKey, ReviewedSuggestion> suggestions, EventReview review, Func<SuggestionKey, bool> kept)
    {
        var events = slice.Events.Where(kept).Select(key => suggestions[key]).ToList();
        if (slice.Events.Any() && events.Count == 0)
        {
            return null;
        }

        string NameOf(SuggestionKey key) => suggestions.TryGetValue(key, out var found) ? found.Name.Value : key.Value[(key.Value.IndexOf('/') + 1)..];
        var shapes = events.SelectMany(reviewed => reviewed.Suggestion.Evidence.Shapes.Select(shape => shape.Shape)).Distinct().ToList();
        var rules = events
            .SelectMany(reviewed => reviewed.Suggestion.Notes.Where(note => note.Kind != NoteKind.Observation).Select(note => new AcceptedRule(note.Kind == NoteKind.Constraint ? AcceptedRuleKind.Constraint : AcceptedRuleKind.Rule, note.Text, reviewed.Suggestion.Evidence.Shapes.Select(shape => shape.Shape).FirstOrDefault() ?? CommandShape.None)))
            .Concat(Chosen(review).Where(rule => shapes.Contains(rule.Shape)))
            .Distinct()
            .ToList();

        return new AcceptedSlice(
            slice.Name,
            slice.Type,
            slice.Description,
            shapes,
            slice.Commands,
            [.. events.Select(reviewed => new AcceptedEvent(
                reviewed.Suggestion.Key,
                reviewed.Name,
                reviewed.Suggestion.Mechanical,
                reviewed.Suggestion.Stream,
                reviewed.Suggestion.Properties,
                reviewed.Suggestion.Confidence,
                reviewed.Status == SuggestionStatus.Suggested))],
            slice.ReadModels,
            [.. slice.Projections.Select(projection => new AcceptedProjection(projection.Name, [.. projection.Events.Where(kept).Select(key => new EventName(NameOf(key)))]))],
            [.. slice.Constraints.Where(constraint => kept(constraint.Event)).Select(constraint => new AcceptedConstraint(constraint.Name, constraint.Property, NameOf(constraint.Event)))],
            rules,
            events.Exists(reviewed => reviewed.Status == SuggestionStatus.Suggested));
    }

    static IEnumerable<AcceptedRule> Chosen(EventReview review) =>
        review.Choices
            .Select(choice => (Choice: choice, Boundary: review.Boundaries.FirstOrDefault(boundary => boundary.Key == choice.Key)))
            .Where(item => item.Boundary is not null)
            .Select(item => new AcceptedRule(AcceptedRuleKind.Boundary, item.Choice.Rule, item.Boundary!.Shape));
}
