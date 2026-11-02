// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.RegularExpressions;
using Cratis.Prologue.Workbench.Events.Questions;

namespace Cratis.Prologue.Workbench.Events.Uncaptured;

/// <summary>
/// What a person said writes with no command are, and what each answer means for the model.
/// </summary>
public static partial class UncorrelatedAnswers
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

    [GeneratedRegex(@"\b(not|no|never|isn't|aren't|wasn't|don't|doesn't|didn't|won't|can't|cannot)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 200)]
    private static partial Regex NegationPattern { get; }

    [GeneratedRegex(@"^\s*(manual\s+)?corrections?\b|\b(staff|people|users?|operators?|admins?|someone|we|they)\b.{0,30}\bcorrect(s|ed|ing)?\b|\b(manual|manually)\b.{0,20}\bcorrect", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 200)]
    private static partial Regex CorrectionPattern { get; }

    /// <summary>
    /// Gets the key of the question about a table.
    /// </summary>
    /// <param name="schema">The schema of the table.</param>
    /// <param name="table">The table.</param>
    /// <returns>The key.</returns>
    public static QuestionKey KeyOf(string schema, string table) => $"uncorrelated|{schema}.{table}".ToLowerInvariant();

    /// <summary>
    /// Reads what an answer means. A choice is read by its exact label. Free text is a description of what the writes are, so it means a job,
    /// unless it clearly says staff correct the data: a negation ("that's not correct, it's a job") or a passing use of the word is never taken as a correction.
    /// </summary>
    /// <param name="answer">The answer: a choice or free text.</param>
    /// <returns>What the writes are according to the answer.</returns>
    public static UncorrelatedMeaning Mean(string answer)
    {
        var text = answer.Trim();
        if (text.Equals(Ignore, StringComparison.OrdinalIgnoreCase) || text.StartsWith("ignore", StringComparison.OrdinalIgnoreCase))
        {
            return UncorrelatedMeaning.Ignored;
        }

        if (text.Equals(StaffCorrection, StringComparison.OrdinalIgnoreCase))
        {
            return UncorrelatedMeaning.Correction;
        }

        return text.Equals(ScheduledJob, StringComparison.OrdinalIgnoreCase) || Negated(text) || !ClearlyACorrection(text)
            ? UncorrelatedMeaning.Job
            : UncorrelatedMeaning.Correction;
    }

    static bool Negated(string text) => NegationPattern.IsMatch(text);

    static bool ClearlyACorrection(string text) => CorrectionPattern.IsMatch(text);
}
