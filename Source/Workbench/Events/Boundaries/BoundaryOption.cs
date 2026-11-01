// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Events.Boundaries;

/// <summary>
/// One way to hold a consistency boundary.
/// </summary>
/// <param name="Key">The identity of the option within its boundary.</param>
/// <param name="Label">What choosing it means, in words.</param>
/// <param name="Rule">The rule or constraint the choice adds to the accepted model.</param>
public record BoundaryOption(string Key, string Label, string Rule);
