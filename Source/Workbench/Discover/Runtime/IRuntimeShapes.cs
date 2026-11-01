// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Discover.Runtime;

/// <summary>
/// The seam between what the code says and what the running system did. Discover asks it what was captured for a prologue and
/// matches the answer against its findings; whatever projects captures into per-shape counts implements it.
/// </summary>
public interface IRuntimeShapes
{
    /// <summary>
    /// Observes what was captured for a prologue, emitting again whenever it changes.
    /// </summary>
    /// <param name="prologue">The prologue.</param>
    /// <returns>The evidence; it emits at least once so a page is never left waiting on a prologue with no captures.</returns>
    IObservable<RuntimeEvidence> Observe(PrologueId prologue);
}
