// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Extractor.Shadowing;

/// <summary>
/// The limits that keep shadowing from ever hurting the system it mirrors.
/// </summary>
public static class ShadowLimits
{
    /// <summary>
    /// The largest request body that is mirrored, in bytes. A larger body is not buffered and the request is reported as skipped.
    /// </summary>
    public const int MaximumBodyBytes = 1024 * 1024;

    /// <summary>
    /// How many mirrored requests are in flight at once.
    /// </summary>
    public const int MaximumConcurrency = 8;

    /// <summary>
    /// How many mirrored requests wait for a free slot. Beyond this the request is not mirrored and nothing is reported.
    /// </summary>
    public const int QueueCapacity = 512;

    /// <summary>
    /// How long the shadow destination has to answer.
    /// </summary>
    public static readonly TimeSpan ShadowTimeout = TimeSpan.FromSeconds(10);

    /// <summary>
    /// How long the Receiver has to take an observation.
    /// </summary>
    public static readonly TimeSpan ReportTimeout = TimeSpan.FromSeconds(5);
}
