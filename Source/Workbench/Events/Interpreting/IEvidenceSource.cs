// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Events.Interpreting;

/// <summary>
/// Gathers what an interpretation starts from: the captures from the capture store, and the Workbench's own evidence beside them.
/// </summary>
public interface IEvidenceSource
{
    /// <summary>
    /// Gathers the input for a prologue.
    /// </summary>
    /// <param name="prologue">The prologue.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The input.</returns>
    Task<InterpretationInput> Load(PrologueId prologue, CancellationToken cancellationToken);
}
