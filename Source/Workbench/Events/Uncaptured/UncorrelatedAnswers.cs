// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Events.Questions;

namespace Cratis.Prologue.Workbench.Events.Uncaptured;

/// <summary>
/// What a person said writes with no command are, and what each answer means for the model.
/// </summary>
public static class UncorrelatedAnswers
{
    /// <summary>
    /// The answer for a job that runs on a schedule.
    /// </summary>
    public const string ScheduledJob = "A scheduled job";

    /// <summary>
    /// The answer for people correcting data by hand.
    /// </summary>
    public const string StaffCorrection = "Staff correcting data";

    /// <summary>
    /// The answer for writes that are not part of the model.
    /// </summary>
    public const string Ignore = "Ignore";

    /// <summary>
    /// Gets the answers on offer.
    /// </summary>
    public static IReadOnlyList<QuestionOption> Choices { get; } =
    [
        new(ScheduledJob, "Suggest an event for what the job changes"),
        new(StaffCorrection, "Suggest a correction command and its event"),
        new(Ignore, "These writes are not part of the model")
    ];

    /// <summary>
    /// Gets the key of the question about a table.
    /// </summary>
    /// <param name="schema">The schema of the table.</param>
    /// <param name="table">The table.</param>
    /// <returns>The key.</returns>
    public static QuestionKey KeyOf(string schema, string table) => $"uncorrelated|{schema}.{table}".ToLowerInvariant();

    /// <summary>
    /// Reads what an answer means.
    /// </summary>
    /// <param name="answer">The answer: a choice or free text.</param>
    /// <returns>What the writes are according to the answer.</returns>
    public static UncorrelatedMeaning Mean(string answer) => answer.Trim() switch
    {
        var text when text.Equals(Ignore, StringComparison.OrdinalIgnoreCase) || text.StartsWith("ignore", StringComparison.OrdinalIgnoreCase) => UncorrelatedMeaning.Ignored,
        var text when text.Equals(StaffCorrection, StringComparison.OrdinalIgnoreCase) || text.Contains("correct", StringComparison.OrdinalIgnoreCase) => UncorrelatedMeaning.Correction,
        _ => UncorrelatedMeaning.Job
    };
}
