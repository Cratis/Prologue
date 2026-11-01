// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Events.Accepted;

/// <summary>
/// A constraint of the accepted model, with the final name of the event it holds on.
/// </summary>
/// <param name="Name">The constraint name.</param>
/// <param name="Property">The property that must be unique.</param>
/// <param name="Event">The final name of the event.</param>
public record AcceptedConstraint(string Name, string Property, EventName Event);
