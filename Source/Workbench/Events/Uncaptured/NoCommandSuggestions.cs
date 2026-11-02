// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Interpretation;
using Cratis.Prologue.Workbench.Decisions;
using Cratis.Prologue.Workbench.Discover;
using Cratis.Prologue.Workbench.Events.Interpreting;
using Cratis.Prologue.Workbench.Events.Questions;
using Cratis.Prologue.Workbench.Events.Suggestions;
using Cratis.Prologue.Workbench.Explore.Uncorrelated;

namespace Cratis.Prologue.Workbench.Events.Uncaptured;

/// <summary>
/// Turns the writes that arrived with no HTTP command into suggested events: one tied to the job Discover found in the code, or one a person
/// said the writes are, or a question when nothing explains them.
/// </summary>
public static class NoCommandSuggestions
{
    /// <summary>
    /// The module the events of writes with no command are keyed under, so a suggestion keeps its identity whatever Discover finds later.
    /// </summary>
    public const string KeyModule = "Background";

    /// <summary>
    /// Suggests the events and raises the questions.
    /// </summary>
    /// <param name="facts">What is known about the prologue.</param>
    /// <param name="answers">The answers people already gave.</param>
    /// <returns>The suggestions with the job each is tied to (none when no job is), and the questions raised.</returns>
    public static (IReadOnlyList<(SuggestedEvent Suggestion, CodeJob? Job)> Suggestions, IReadOnlyList<RaisedQuestion> Questions) From(EvidenceFacts facts, IReadOnlyList<QuestionAnswer> answers)
    {
        var suggestions = new List<(SuggestedEvent, CodeJob?)>();
        var questions = new List<RaisedQuestion>();
        foreach (var table in facts.Uncorrelated.Where(table => table.Transactions > 0))
        {
            var answer = answers.FirstOrDefault(candidate => candidate.Key == UncorrelatedAnswers.KeyOf(table.Write.Schema.Value, table.Write.Table.Value));
            var meaning = answer is null ? (UncorrelatedMeaning?)null : UncorrelatedAnswers.Mean(answer.Answer);
            var job = JobMatching.For(table.Write.Table.Value, facts.Jobs);
            if (meaning == UncorrelatedMeaning.Ignored)
            {
                continue;
            }

            if (job is null && meaning is null)
            {
                if (table.Transactions >= UncorrelatedQuestions.MinimumTransactions)
                {
                    questions.Add(UncorrelatedQuestions.For(table));
                }

                continue;
            }

            suggestions.Add((Suggest(table, job, meaning == UncorrelatedMeaning.Correction, answer?.Answer ?? string.Empty, facts), job));
        }

        return (suggestions, questions);
    }

    static SuggestedEvent Suggest(UncorrelatedTable table, CodeJob? job, bool correction, string answer, EvidenceFacts facts)
    {
        var entity = TableEvents.Entity(table.Write.Table.Value);
        var operation = table.Write.Operations.OrderByDescending(runs => runs.Runs).Select(runs => runs.Operation).FirstOrDefault(ChangeOperation.Update);
        var mechanical = correction ? $"{entity}Corrected" : Naming.EventName(entity, operation);
        var refined = correction ? mechanical : JobNaming.EventName(entity, job is null ? string.Empty : $"{job.Name} {job.Detail}", operation);
        var code = Located(job);
        var columns = table.Write.Columns.Select(column => column.Value).Where(column => !column.Equals("Id", StringComparison.OrdinalIgnoreCase) && !column.Equals($"{entity}Id", StringComparison.OrdinalIgnoreCase)).Distinct().Order();
        var module = ModuleOf(job);
        return new SuggestedEvent(
            CaptureAttribution.KeyOf(KeyModule, mechanical),
            mechanical,
            refined,
            module,
            entity,
            [.. columns.Select(column => new SuggestedProperty(Naming.Pascalize(column), Naming.InferType(column), true, 0))],
            SuggestionConfidence.ForUncorrelated(table.Transactions, job is not null),
            new SuggestionEvidence([], [.. table.Write.Operations.Select(runs => new TableEvidence(table.Write.Schema.Value, table.Write.Table.Value, runs.Operation.ToString(), table.Write.Columns.Select(column => column.Value)))], code, []),
            Notes(table, job, correction, answer),
            SuggestionOrigin.NoCommand);
    }

    static string ModuleOf(CodeJob? job)
    {
        if (job is null || job.Module.Value.Length == 0)
        {
            return KeyModule;
        }

        return Naming.Pascalize(job.Module.Value);
    }

    static List<CodeEvidence> Located(CodeJob? job)
    {
        if (job is null || job.File.Value.Length == 0)
        {
            return [];
        }

        return [new(new SourceFilePath(job.File.Value), new Decisions.LineNumber(job.Line.Value), job.Name)];
    }

    static IReadOnlyList<SuggestionNote> Notes(UncorrelatedTable table, CodeJob? job, bool correction, string answer)
    {
        var happening = $"{table.Write.Table.Value} changes {table.Transactions} times with no command{UncorrelatedQuestions.When(table)} (UTC).";
        return [new(NoteKind.Observation, $"{happening} {Source(job, correction, answer)}")];
    }

    static string Source(CodeJob? job, bool correction, string answer)
    {
        if (job is not null)
        {
            return $"Discover found a job in the code that explains it: {job.Name}.";
        }

        return correction ? "You said staff correct this data. A correction command would own the event." : $"No code explains it. You answered: {answer}.";
    }
}
