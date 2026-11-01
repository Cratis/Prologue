// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Events.Accepted;

/// <summary>
/// A projection of the accepted model, with the final names of the events it consumes.
/// </summary>
/// <param name="Name">The projection name.</param>
/// <param name="Events">The final names of the events it consumes.</param>
public record AcceptedProjection(string Name, IEnumerable<EventName> Events);
