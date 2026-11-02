// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Events.Suggestions;

namespace Cratis.Prologue.Workbench.Events.Questions;

/// <summary>
/// Reads the questions the Workbench raised from its own evidence out of the review.
/// </summary>
public static class RaisedQuestions
{
    /// <summary>
    /// Gets the raised questions nobody answered.
    /// </summary>
    /// <param name="review">The review; null when nothing was ever interpreted.</param>
    /// <returns>The open questions.</returns>
    public static IEnumerable<RaisedQuestion> Open(EventReview? review) =>
        (review?.Raised ?? []).Where(raised => (review?.Answers ?? []).All(answer => answer.Key != raised.Key));

    /// <summary>
    /// Finds a raised question that waits for an answer.
    /// </summary>
    /// <param name="review">The review; null when nothing was ever interpreted.</param>
    /// <param name="id">The question.</param>
    /// <returns>The question; null when it is not raised or is answered already.</returns>
    public static RaisedQuestion? Find(EventReview? review, QuestionId id) => Open(review).FirstOrDefault(raised => raised.Id == id);
}
