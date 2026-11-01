// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Explore.Journeys;

/// <summary>
/// An identity found in a request path.
/// </summary>
/// <param name="Kind">What it identifies.</param>
/// <param name="Id">The identity.</param>
public record PathIdentity(EntityKind Kind, EntityId Id);
