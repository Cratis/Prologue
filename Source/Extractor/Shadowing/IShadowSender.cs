// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Extractor.Shadowing;

/// <summary>
/// Sends a copy of a request to the shadow destination.
/// </summary>
public interface IShadowSender
{
    /// <summary>
    /// Sends the copy and reads only the status of the answer.
    /// </summary>
    /// <param name="job">The request to copy.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The status the shadow destination answered with, or null when it could not be reached or did not answer in time.</returns>
    Task<int?> Send(ShadowJob job, CancellationToken cancellationToken);
}
