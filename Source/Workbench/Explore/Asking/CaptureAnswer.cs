// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.ReadModels;
using Cratis.Prologue.Workbench.AI.Asking;
using Cratis.Prologue.Workbench.Captures;
using Cratis.Prologue.Workbench.Explore.Journeys;
using Cratis.Prologue.Workbench.Explore.Matrix;
using Cratis.Prologue.Workbench.Explore.Scope;
using Cratis.Prologue.Workbench.Explore.Uncorrelated;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Explore.Asking;

/// <summary>
/// The answer to a question about a prologue's captures. The question and a bounded summary of the captures (counts, shares,
/// table and column names; never a row value) go to the Interpreter agent through Cratis.AI, and the answer must cite the
/// command shapes it rests on.
/// </summary>
/// <param name="Question">The question as asked.</param>
/// <param name="Status">How asking ended.</param>
/// <param name="Text">The answer; empty unless <paramref name="Status"/> is <see cref="AnswerStatus.Answered"/>.</param>
/// <param name="Citations">The command shapes the answer cites, all of them captured.</param>
/// <param name="Reason">Why there is no answer, in words a person can act on; empty when there is one.</param>
[ReadModel]
public record CaptureAnswer(string Question, AnswerStatus Status, string Text, IEnumerable<CommandShape> Citations, string Reason)
{
    /// <summary>
    /// Asks the Interpreter agent a question about the captures of a prologue.
    /// </summary>
    /// <param name="id">The prologue.</param>
    /// <param name="question">The question.</param>
    /// <param name="readModels">Reads what was captured.</param>
    /// <param name="asker">Asks the agent.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The answer, or why there is none.</returns>
    public static async Task<CaptureAnswer> AskAboutCaptures(PrologueId id, string question, IReadModels readModels, IAgentAsker asker, CancellationToken cancellationToken)
    {
        var key = (EventSourceId)id;
        return await CaptureAnswers.Compose(
            question,
            await readModels.GetInstanceById<CommandMatrix>(key),
            await readModels.GetInstanceById<UncorrelatedWrites>(key),
            await readModels.GetInstanceById<Journeys.Journeys>(key),
            await readModels.GetInstanceById<InterpretationScope>(key),
            asker,
            cancellationToken);
    }
}
