// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Captures.Live;

namespace Cratis.Prologue.Workbench.Decisions;

/// <summary>
/// Describes a decision from what was captured alone, which is the reason when no decision point matches.
/// </summary>
public static class ObservedPattern
{
    /// <summary>
    /// Describes what a capture shows. A refusal that wrote nothing says so, a refusal that wrote something names the tables,
    /// and an acceptance names what it wrote. Unique indexes and foreign keys are not named because a capture holds no schema.
    /// </summary>
    /// <param name="capture">The capture.</param>
    /// <returns>The description.</returns>
    public static RuleText Describe(RecordedCapture capture)
    {
        var writes = capture.Transactions
            .SelectMany(transaction => transaction.Tables)
            .Select(change => $"{change.Operation.ToString().ToUpperInvariant()} {change.Table.Value}")
            .Distinct()
            .ToList();

        var written = string.Join(", ", writes);
        var status = capture.Http.Status.Value;

        return (capture.Outcome, writes.Count) switch
        {
            (DecisionOutcome.Refused, 0) => $"nothing was written ({status})",
            (DecisionOutcome.Refused, _) => $"it answered {status} although it wrote {written}",
            (_, 0) => "nothing needed to be written",
            _ => $"it wrote {written}"
        };
    }
}
