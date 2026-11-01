// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Events.Boundaries;

/// <summary>
/// The option a person chose for a consistency boundary.
/// </summary>
/// <param name="Key">The boundary.</param>
/// <param name="Option">The key of the option chosen.</param>
/// <param name="Rule">The rule or constraint the choice adds to the accepted model.</param>
public record BoundaryChoice(BoundaryKey Key, string Option, string Rule);
