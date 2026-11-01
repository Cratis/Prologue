// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Interpretation;
using Cratis.Prologue.Workbench.Events.Suggestions;

namespace Cratis.Prologue.Workbench.Events.Questions;

/// <summary>
/// Finds the suggestions a question of the interpreter is about, so the page can say which events wait for an answer.
/// </summary>
public static class QuestionLinks
{
    /// <summary>
    /// Gets the suggestions a question is about: the ones whose name, mechanical name or command shapes the question's prompt or context mentions.
    /// </summary>
    /// <param name="question">The question.</param>
    /// <param name="suggestions">The suggestions.</param>
    /// <returns>The suggestions the question affects.</returns>
    public static IEnumerable<SuggestionKey> Affected(InterpreterQuestion question, IEnumerable<ReviewedSuggestion> suggestions)
    {
        var text = $"{question.Prompt} {question.Context} {string.Join(' ', question.Choices.Select(choice => $"{choice.Label} {choice.Description}"))}";
        return suggestions.Where(suggestion => Mentions(text, suggestion)).Select(suggestion => suggestion.Suggestion.Key);
    }

    static bool Mentions(string text, ReviewedSuggestion suggestion)
    {
        var terms = new List<string> { suggestion.Name.Value, suggestion.Suggestion.Mechanical.Value, suggestion.Suggestion.Refined.Value };
        terms.AddRange(suggestion.Suggestion.Evidence.Shapes.Select(shape => shape.Shape.Value));
        return terms.Exists(term => term.Length >= 4 && text.Contains(term, StringComparison.OrdinalIgnoreCase));
    }
}
