// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Reactive.Subjects;
using Cratis.Prologue.Interpretation;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Events.Questions;

/// <summary>
/// Represents an in-memory <see cref="IInterpretationSessions"/>.
/// </summary>
public sealed class InterpretationSessions : IInterpretationSessions, IDisposable
{
    readonly ConcurrentDictionary<PrologueId, ParkedInterpretation> _parked = new();
    readonly ConcurrentDictionary<PrologueId, BehaviorSubject<IReadOnlyList<InterpreterQuestion>>> _questions = new();

    /// <inheritdoc/>
    public void Park(PrologueId prologue, ParkedInterpretation parked)
    {
        _parked[prologue] = parked;
        SubjectOf(prologue).OnNext(parked.State.PendingQuestions);
    }

    /// <inheritdoc/>
    public ParkedInterpretation? Take(PrologueId prologue)
    {
        var taken = _parked.TryRemove(prologue, out var parked) ? parked : null;
        SubjectOf(prologue).OnNext([]);
        return taken;
    }

    /// <inheritdoc/>
    public IReadOnlyList<InterpreterQuestion> Pending(PrologueId prologue) => SubjectOf(prologue).Value;

    /// <inheritdoc/>
    public IObservable<IReadOnlyList<InterpreterQuestion>> Observe(PrologueId prologue) => SubjectOf(prologue);

    /// <inheritdoc/>
    public AnswerOutcome Answer(PrologueId prologue, QuestionId question, string answer)
    {
        if (!_parked.TryGetValue(prologue, out var parked))
        {
            return new(null, false);
        }

        var asked = parked.State.PendingQuestions.FirstOrDefault(pending => pending.Id == question.Value);
        if (asked is null)
        {
            return new(null, false);
        }

        var state = parked.State.Answered(new InterpreterAnswer(question.Value, answer));
        _parked[prologue] = parked with { State = state };
        SubjectOf(prologue).OnNext(state.PendingQuestions);
        return new(asked, state.PendingQuestions.Count == 0);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        foreach (var subject in _questions.Values)
        {
            subject.Dispose();
        }
    }

    BehaviorSubject<IReadOnlyList<InterpreterQuestion>> SubjectOf(PrologueId prologue) =>
        _questions.GetOrAdd(prologue, _ => new BehaviorSubject<IReadOnlyList<InterpreterQuestion>>([]));
}
