// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Events.NotEvents;

/// <summary>
/// Something the interpreter does not turn into an event, and why.
/// </summary>
/// <param name="Kind">Why.</param>
/// <param name="Subject">What it is: a column such as Inventory.AvailableCount or a command shape.</param>
/// <param name="Reason">The reason, in words.</param>
public record NotEvent(NotEventKind Kind, string Subject, string Reason);
