// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Storage;

/// <summary>
/// Defines where correlated Prologue captures are persisted. Shared by everything that ingests captures — the
/// Prologue API endpoint the engine posts to, and the in-process import of file-based capture results.
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

    /// <summary>
    /// Gets all captures belonging to a specific Prologue, ordered by when they occurred.
    /// </summary>
    /// <param name="prologueId">The identifier of the Prologue to get captures for.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The captures associated with the Prologue.</returns>
    Task<IReadOnlyList<Capture>> GetForPrologue(Guid prologueId, CancellationToken cancellationToken = default);
}
