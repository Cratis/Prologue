// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Interpretation;
using Cratis.Prologue.Workbench.Discover.Findings;

namespace Cratis.Prologue.Workbench.Events.Questions;

/// <summary>
/// Relates the interpreter's questions to what people already answered.
/// </summary>
public static class QuestionKeys
{
    /// <summary>
    /// Gets the key of a question: its prompt, normalized so case, punctuation and spacing do not make the same question look new.
    /// </summary>
    /// <param name="prompt">The prompt.</param>
    /// <returns>The key.</returns>
    public static QuestionKey For(string prompt) => FindingKeys.Normalize(prompt);

    /// <summary>
    /// Finds the answer already given to a question, when the question is one that was answered before.
    /// </summary>
    /// <param name="question">The question.</param>
    /// <param name="answers">The answers given.</param>
    /// <returns>The answer; null when nobody answered this question.</returns>
    public static QuestionAnswer? AnswerTo(InterpreterQuestion question, IEnumerable<QuestionAnswer> answers) =>
        answers.FirstOrDefault(answer => answer.Key == For(question.Prompt));
}
