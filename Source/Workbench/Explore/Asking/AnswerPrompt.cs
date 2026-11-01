// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Explore.Asking;

/// <summary>
/// Builds the prompt that asks an agent a question about the captures.
/// </summary>
public static class AnswerPrompt
{
    /// <summary>
    /// Builds the prompt.
    /// </summary>
    /// <param name="question">The question as asked.</param>
    /// <param name="evidence">The summary of the captures, see <see cref="CaptureEvidence"/>.</param>
    /// <returns>The prompt.</returns>
    public static string For(string question, string evidence) =>
        $$"""
        You help a person understand what an existing system does, from HTTP commands, database writes and telemetry that were
        captured while the system ran. You only have the evidence below. It holds counts, shares, table and column names; there
        are no row values, so never claim to know one.

        Answer the question in two to five plain sentences, using the counts and shares in the evidence. Say so when the evidence
        cannot answer it. Cite every command shape your answer rests on, written exactly as it appears in the evidence.

        Reply with JSON only, in this shape: {"answer": "...", "citations": ["POST /api/example/{id}"]}

        Evidence:
        {{evidence}}

        Question: {{question.Trim()}}
        """;
}
