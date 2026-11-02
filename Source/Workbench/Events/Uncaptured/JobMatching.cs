// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;
using Cratis.Prologue.Interpretation;
using Cratis.Prologue.Workbench.Events.Interpreting;

namespace Cratis.Prologue.Workbench.Events.Uncaptured;

/// <summary>
/// Finds the job in the code that explains writes to a table by reading the table's name in what Discover said about the job.
/// </summary>
public static class JobMatching
{
    /// <summary>
    /// Finds the job that writes a table. A job whose description names the table as it is, or the entity it holds, matches; the table
    /// as it is beats the entity, and a job with a place in the code beats one without.
    /// </summary>
    /// <param name="table">The table.</param>
    /// <param name="jobs">The jobs Discover found.</param>
    /// <returns>The best matching job; null when no job mentions the table.</returns>
    public static CodeJob? For(string table, IReadOnlyList<CodeJob> jobs)
    {
        var whole = Words(table);
        var entity = Words(TableEvents.Entity(table));
        var scored = jobs
            .Select(job => (Job: job, Score: Score($"{job.Name} {job.Detail}", whole, entity) + (job.File.Value.Length > 0 ? 1 : 0)))
            .Where(item => item.Score >= 2)
            .OrderByDescending(item => item.Score)
            .ToList();
        return scored.Count == 0 ? null : scored[0].Job;
    }

    /// <summary>
    /// Splits a name into lower-case words: on anything that is not a letter or digit, and on the humps of camel case.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>The words, joined by single spaces.</returns>
    public static string Words(string text)
    {
        var builder = new StringBuilder(text.Length + 4);
        for (var index = 0; index < text.Length; index++)
        {
            var character = text[index];
            if (!char.IsLetterOrDigit(character))
            {
                AppendSpace(builder);
                continue;
            }

            if (char.IsUpper(character) && index > 0 && (char.IsLower(text[index - 1]) || char.IsDigit(text[index - 1])))
            {
                AppendSpace(builder);
            }

            builder.Append(char.ToLowerInvariant(character));
        }

        return builder.ToString().Trim();
    }

    static int Score(string description, string whole, string entity)
    {
        var words = $" {Words(description)} ";
        if (whole.Length > 0 && words.Contains($" {whole} ", StringComparison.Ordinal))
        {
            return 4;
        }

        return entity.Length > 0 && words.Contains($" {entity} ", StringComparison.Ordinal) ? 2 : 0;
    }

    static void AppendSpace(StringBuilder builder)
    {
        if (builder.Length > 0 && builder[^1] != ' ')
        {
            builder.Append(' ');
        }
    }
}
