// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Events.Interpreting;

/// <summary>
/// Holds how the interpretations are going.
/// </summary>
public interface IInterpretationProgress
{
    /// <summary>
    /// Observes the progress of a prologue; its current progress first.
    /// </summary>
    /// <param name="prologue">The prologue.</param>
    /// <returns>The progress, pushed whenever it changes.</returns>
    IObservable<InterpretationProgress> Observe(PrologueId prologue);

    /// <summary>
    /// Gets the progress of a prologue.
    /// </summary>
    /// <param name="prologue">The prologue.</param>
    /// <returns>The progress; idle when nothing was run.</returns>
    InterpretationProgress Get(PrologueId prologue);

    /// <summary>
    /// Sets the progress of a prologue.
    /// </summary>
    /// <param name="progress">The progress.</param>
    void Set(InterpretationProgress progress);
}
