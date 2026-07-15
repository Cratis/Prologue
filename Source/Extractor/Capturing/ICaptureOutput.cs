// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Extractor.Capturing;

/// <summary>
/// Defines where the extractor writes correlated captures. Implementations are the configurable outputs — the Prologue
/// API or rolling JSON files.
/// </summary>
public interface ICaptureOutput
{
    /// <summary>
    /// Writes a batch of correlated captures.
    /// </summary>
    /// <param name="captures">The captures to write.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task Write(IReadOnlyList<Capture> captures, CancellationToken cancellationToken = default);
}
