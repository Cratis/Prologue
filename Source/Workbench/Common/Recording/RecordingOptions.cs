// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Common.Recording;

/// <summary>
/// Configuration for the background workers that record captures and parity observations, bound from <c language="text">Prologue:Recording</c>.
/// </summary>
public class RecordingOptions
{
    /// <summary>
    /// The configuration section the options are bound from.
    /// </summary>
    public const string SectionName = "Prologue:Recording";

    /// <summary>
    /// Gets or sets how long one item may take to record before the worker gives up on it and moves to the next.
    /// The stored capture or observation stays; only its recording for the Workbench is lost.
    /// </summary>
    public TimeSpan ItemTimeout { get; set; } = TimeSpan.FromSeconds(10);
}
