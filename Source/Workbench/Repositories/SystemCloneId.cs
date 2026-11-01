// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Systems;

namespace Cratis.Prologue.Workbench.Repositories;

/// <summary>
/// The identity of the stream of facts about the clone of one system's repository.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record SystemCloneId(Guid Value) : EventSourceId<Guid>(Value)
{
    /// <summary>
    /// Implicitly converts a <see cref="Guid"/> to a <see cref="SystemCloneId"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator SystemCloneId(Guid value) => new(value);

    /// <summary>
    /// Gets the identity of the clone of a system.
    /// </summary>
    /// <param name="system">The system.</param>
    /// <returns>The identity.</returns>
    public static SystemCloneId For(SystemId system) => new(system.Value);
}
