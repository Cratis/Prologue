// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Rebuild.Parity;

/// <summary>
/// Records the parity observations the Extractor reports to the Receiver.
/// </summary>
public interface IParityRecorder
{
    /// <summary>
    /// Records an observation for a prologue. A failure is logged and never thrown; it is reported to the caller, which may try again because
    /// recording the same observation twice records it once.
    /// </summary>
    /// <param name="prologue">The prologue the Extractor is configured for.</param>
    /// <param name="observation">The observation.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>True when the observation is recorded or was refused for good (trying again would not change that); false when recording failed and may succeed later.</returns>
    Task<bool> Record(Guid prologue, ParityObservation observation, CancellationToken cancellationToken = default);
}
