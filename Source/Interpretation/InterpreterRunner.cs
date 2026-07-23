// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Interpretation;

/// <summary>
/// Drives an interpreter session synchronously to its end — the flow a CLI or batch host uses. It loops
/// <see cref="IInterpreterSession.Proceed"/> and, whenever the session parks awaiting answers, surfaces the
/// pending questions through <see cref="IInterpreterCallbacks.OnQuestion"/> ONE AT A TIME, feeding each answer
/// back before asking the next. Status transitions reach the host through the session's own status callback —
/// wire <see cref="IInterpreterCallbacks.OnStatusChanged"/> into the factory when creating the session.
/// </summary>
public class InterpreterRunner
{
    /// <summary>
    /// Runs the session until it is <see cref="InterpreterStatus.Completed"/> or
    /// <see cref="InterpreterStatus.Failed"/>, answering every question through the callbacks along the way.
    /// </summary>
    /// <param name="session">The session to drive.</param>
    /// <param name="callbacks">The callbacks answering the questions.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the run.</param>
    /// <returns>The final <see cref="InterpreterSessionState"/>.</returns>
    public async Task<InterpreterSessionState> Run(IInterpreterSession session, IInterpreterCallbacks callbacks, CancellationToken cancellationToken = default)
    {
        var state = session.State;

        // GeneratingScreenplay is a host-reported status the session itself never enters; treating it as terminal
        // guards the loop against a state that would otherwise never progress.
        while (state.Status is not (InterpreterStatus.Completed or InterpreterStatus.Failed or InterpreterStatus.GeneratingScreenplay))
        {
            state = await session.Proceed(cancellationToken);
            while (state.PendingQuestions.Count > 0)
            {
                var answer = await callbacks.OnQuestion(state.PendingQuestions[0]);
                state = session.Answer(answer);
            }
        }

        return state;
    }
}
