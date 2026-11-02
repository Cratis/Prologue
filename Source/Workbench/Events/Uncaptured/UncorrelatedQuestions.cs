// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Cryptography;
using System.Text;
using Cratis.Prologue.Workbench.Events.Questions;
using Cratis.Prologue.Workbench.Explore.Uncorrelated;

namespace Cratis.Prologue.Workbench.Events.Uncaptured;

/// <summary>
/// Writes the question about a table that changes with no command and no job in the code explains.
/// </summary>
public static class UncorrelatedQuestions
{
    /// <summary>
    /// The fewest transactions a table must change in before it is worth a question; one stray write is not a pattern.
    /// </summary>
    public const int MinimumTransactions = 2;

    /// <summary>
    /// Writes the question.
    /// </summary>
    /// <param name="table">The table with the transactions that wrote it.</param>
    /// <returns>The question.</returns>
    public static RaisedQuestion For(UncorrelatedTable table)
    {
        var name = table.Write.Table.Value;
        var key = UncorrelatedAnswers.KeyOf(table.Write.Schema.Value, name);
        var prompt = $"{name} changes {table.Transactions} times with no command{When(table)}. What is that?";
        var operations = string.Join(", ", table.Write.Operations.Select(operation => $"{operation.Operation.ToString().ToLowerInvariant()} ×{operation.Runs}"));
        var context = $"Writes: {operations}. Hours are UTC. Discover found no job in the code that writes {name}.";
        return new RaisedQuestion(Identify(key), key, prompt, context, UncorrelatedAnswers.Choices);
    }

    /// <summary>
    /// Describes when the writes happen: the hour most of them fall in, or that they are spread across the day.
    /// </summary>
    /// <param name="table">The table.</param>
    /// <returns>The phrase, starting with a comma.</returns>
    public static string When(UncorrelatedTable table)
    {
        var top = table.Hours.OrderByDescending(hour => hour.Transactions).ThenBy(hour => hour.Hour).FirstOrDefault();
        return top is not null && top.Transactions * 2 >= table.Transactions ? $", mostly around {top.Hour:00}:00" : ", spread across the day";
    }

    static QuestionId Identify(QuestionKey key) => new(new Guid(SHA256.HashData(Encoding.UTF8.GetBytes(key.Value))[..16]));
}
