// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Linq;
using Cratis.Chronicle.ReadModels;
using Cratis.Prologue.Workbench.Captures;
using Cratis.Prologue.Workbench.Explore.Matrix;
using Cratis.Prologue.Workbench.Explore.Scope;
using Cratis.Prologue.Workbench.Explore.Uncorrelated;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Explore.Asking;

/// <summary>
/// A question worth asking about a prologue's captures, with the answer the numbers give on their own. When no AI provider is
/// configured the answer is what Explore shows; when one is, the question is sent to the Interpreter agent.
/// </summary>
/// <param name="Question">The question.</param>
/// <param name="Answer">The answer worked out from the command matrix, without an agent.</param>
/// <param name="Citations">The command shapes the answer rests on.</param>
[ReadModel]
public record SuggestedQuestion(string Question, string Answer, IEnumerable<CommandShape> Citations)
{
    /// <summary>
    /// Observes the questions suggested for a prologue. They change when captures arrive or the scope of interpretation does.
    /// </summary>
    /// <param name="id">The prologue.</param>
    /// <param name="readModels">Observes the stored matrix.</param>
    /// <returns>The suggestions.</returns>
    public static ISubject<IEnumerable<SuggestedQuestion>> ObserveSuggestedQuestions(PrologueId id, IReadModels readModels)
    {
        var suggestions = new ReplaySubject<IEnumerable<SuggestedQuestion>>(1);
        var matrix = readModels.Observe<CommandMatrix>(item => item.Id == id).Select(items => items.FirstOrDefault());
        var uncorrelated = readModels.Observe<UncorrelatedWrites>(item => item.Id == id).Select(items => items.FirstOrDefault());
        var scope = readModels.Observe<InterpretationScope>(item => item.Id == id).Select(items => items.FirstOrDefault());
        matrix
            .CombineLatest(uncorrelated, scope, SuggestedQuestions.For)
            .Subscribe(suggestions);
        return suggestions;
    }
}
