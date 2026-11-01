// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Rebuild.Parity;

/// <summary>
/// Records the parity observations the Extractor reports to the Receiver.
/// </summary>
public interface IParityRecorder
{
    /// <summary>
    /// Records an observation for a prologue. A failure is logged and never thrown: the Extractor must not retry on a Workbench problem.
    /// </summary>
    /// <param name="prologue">The prologue the Extractor is configured for.</param>
    /// <param name="observation">The observation.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task Record(Guid prologue, ParityObservation observation, CancellationToken cancellationToken = default);
}
