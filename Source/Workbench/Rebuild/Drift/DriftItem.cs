// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Captures;

namespace Cratis.Prologue.Workbench.Rebuild.Drift;

/// <summary>
/// Something the legacy system does that the accepted model does not cover.
/// </summary>
/// <param name="Key">What identifies the drift.</param>
/// <param name="Kind">How the system changed.</param>
/// <param name="Shape">The command shape.</param>
/// <param name="Column">The column the shape started writing; none for a new shape.</param>
/// <param name="Captures">How many captures showed it.</param>
/// <param name="LastSeen">When it was last captured.</param>
/// <param name="Acknowledged">Whether events were suggested for it.</param>
public record DriftItem(DriftKey Key, DriftKind Kind, CommandShape Shape, WrittenColumn Column, int Captures, DateTimeOffset LastSeen, bool Acknowledged);
