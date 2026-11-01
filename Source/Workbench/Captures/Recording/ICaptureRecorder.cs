// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Captures.Recording;

/// <summary>
/// Records the captures the Receiver stores, so the Workbench sees them.
/// </summary>
public interface ICaptureRecorder
{
    /// <summary>
    /// Records a capture that has already been stored. A failure is logged and never thrown: the stored capture must not be lost.
    /// </summary>
    /// <param name="capture">The stored capture.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task Record(Capture capture, CancellationToken cancellationToken = default);
}
