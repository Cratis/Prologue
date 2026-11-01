// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Extractor.Shadowing;

/// <summary>
/// Accepts mirrored requests without ever making the caller wait.
/// </summary>
public interface IShadowQueue
{
    /// <summary>
    /// Queues a request to mirror.
    /// </summary>
    /// <param name="job">The request.</param>
    /// <returns>True when it was queued; false when the queue is full and the request will not be mirrored.</returns>
    bool TryEnqueue(ShadowJob job);
}
