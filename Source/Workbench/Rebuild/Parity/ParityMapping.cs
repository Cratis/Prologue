// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Captures;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Rebuild.Parity;

/// <summary>
/// Maps an observation the Extractor reported to the command that records it.
/// </summary>
public static class ParityMapping
{
    /// <summary>
    /// Maps an observation to the command recording it. A shape the Extractor did not work out is worked out from the method and path.
    /// </summary>
    /// <param name="prologue">The prologue the Extractor is configured for.</param>
    /// <param name="observation">The observation.</param>
    /// <returns>The command.</returns>
    public static RecordParityObservation ToCommand(Guid prologue, ParityObservation observation) => new(
        new PrologueId(prologue),
        observation.Id,
        observation.Occurred,
        observation.Method.ToUpperInvariant(),
        observation.Path.Split('?', '#')[0],
        string.IsNullOrWhiteSpace(observation.Shape) ? CommandShape.From(observation.Method, observation.Path) : observation.Shape,
        observation.Trace,
        observation.LegacyStatus,
        observation.ShadowStatus,
        observation.Outcome);
}
