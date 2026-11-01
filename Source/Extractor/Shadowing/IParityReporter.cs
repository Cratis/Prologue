// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Extractor.Shadowing;

/// <summary>
/// Reports parity observations to the Receiver.
/// </summary>
public interface IParityReporter
{
    /// <summary>
    /// Reports an observation. A failure is logged and never thrown.
    /// </summary>
    /// <param name="observation">The observation.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task Report(ParityObservation observation, CancellationToken cancellationToken);
}
