// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Events.Interpreting;

/// <summary>
/// A part of a prologue's captures that is interpreted on its own, so a command shape split by response status becomes one candidate per status.
/// </summary>
/// <param name="Captures">The captures of the part, with the captured database schema.</param>
/// <param name="SliceSuffix">What to add to the names of the slices found in the part; empty for the part holding everything not split.</param>
public record CapturePartition(IReadOnlyList<Capture> Captures, string SliceSuffix);
