// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Events.NotEvents;

/// <summary>
/// Why something is not an event.
/// </summary>
public enum NotEventKind
{
    /// <summary>
    /// A value every command touching the table keeps up to date, such as a counter. Derived state: a read model, not an event property.
    /// </summary>
    DerivedState,

    /// <summary>
    /// A nullable column that is set later by its own command. Recorded as its own fact rather than an optional property.
    /// </summary>
    NullableFact,

    /// <summary>
    /// Traffic left out of interpretation in Explore.
    /// </summary>
    ExcludedTraffic
}
