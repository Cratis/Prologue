// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.AI;
using Cratis.Prologue.Workbench.AI.Asking;
using Cratis.Prologue.Workbench.Explore.Matrix;
using Cratis.Prologue.Workbench.Explore.Scope;
using Cratis.Prologue.Workbench.Explore.Uncorrelated;

namespace Cratis.Prologue.Workbench.Explore.Asking;

/// <summary>
/// Composes the answer to a question about the captures from what the Interpreter agent replies.
/// </summary>
public static class CaptureAnswers
{
    /// <summary>
    /// Asks the Interpreter agent and holds its reply to the rule that an answer cites the command shapes it rests on.
    /// </summary>
    /// <param name="question">The question.</param>
    /// <param name="matrix">The command matrix.</param>
    /// <param name="uncorrelated">The writes with no command.</param>
    /// <param name="journeys">The inferred journeys.</param>
    /// <param name="scope">The interpretation scope.</param>
    /// <param name="asker">Asks the agent.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The answer, or why there is none.</returns>
    public static async Task<CaptureAnswer> Compose(
        string question,
        CommandMatrix? matrix,
        UncorrelatedWrites? uncorrelated,
        Journeys.Journeys? journeys,
        InterpretationScope? scope,
        IAgentAsker asker,
        CancellationToken cancellationToken)
    {
        var shapes = CaptureEvidence.Shapes(matrix, scope).Select(profile => profile.Shape).ToList();
        if (shapes.Count == 0)
        {
            return new(question, AnswerStatus.NothingCaptured, string.Empty, [], "Nothing has been captured for this prologue yet");
        }

        var reply = await asker.Ask(PrologueAgents.Interpreter, AnswerPrompt.For(question, CaptureEvidence.Summarize(matrix, uncorrelated, journeys, scope)), cancellationToken);
        if (!reply.Succeeded)
        {
            return new(question, AnswerStatus.Unavailable, string.Empty, [], reply.Reason);
        }

        var parsed = AnswerParsing.Parse(reply.Text, shapes);
        return parsed?.Citations.Any() != true
            ? new(question, AnswerStatus.Uncited, string.Empty, [], "The answer did not cite any command shape that was captured, so it is not shown")
            : new(question, AnswerStatus.Answered, parsed.Text, parsed.Citations, string.Empty);
    }
}
