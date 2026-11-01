// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Explore.Journeys;

/// <summary>
/// A thing that is no longer followed. Remembering it is what keeps it from being counted as a new thing when it shows up again.
/// </summary>
/// <param name="Kind">The kind of thing.</param>
/// <param name="Id">The identity from the request path.</param>
public record EvictedEntity(EntityKind Kind, EntityId Id);
