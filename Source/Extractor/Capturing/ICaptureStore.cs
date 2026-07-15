// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Extractor.Capturing;

/// <summary>
/// Defines where correlated captures are persisted.
/// </summary>
public interface ICaptureStore
{
    /// <summary>
    /// Stores a correlated capture.
    /// </summary>
    /// <param name="capture">The <see cref="Capture"/> to store.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task Store(Capture capture, CancellationToken cancellationToken = default);
}
