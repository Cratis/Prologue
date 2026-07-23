// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Interpretation;

namespace Cratis.Prologue.Interpreter;

/// <summary>
/// Represents the <see cref="IInterpreterCallbacks"/> for the non-interactive batch mode — status transitions are
/// logged as console lines, and questions cannot occur because batch sessions are created with zero question
/// rounds, which tells the language model not to ask and makes the session finalize instead of parking. Should a
/// question surface anyway, it is answered blank immediately so a batch run can never stall.
/// </summary>
public class BatchCallbacks : IInterpreterCallbacks
{
    /// <inheritdoc/>
    public Task<InterpreterAnswer> OnQuestion(InterpreterQuestion question) =>
        Task.FromResult(new InterpreterAnswer(question.Id, string.Empty));

    /// <inheritdoc/>
    public void OnStatusChanged(InterpreterStatus status) => Console.WriteLine($"Status: {status}");
}
