// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Systems;

namespace Cratis.Prologue.Workbench.Repositories;

/// <summary>
/// The identity of the connection to a repository host. There is one connection per host.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record RepositoryConnectionId(string Value) : EventSourceId<string>(Value)
{
    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="RepositoryConnectionId"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator RepositoryConnectionId(string value) => new(value);

    /// <summary>
    /// Gets the identity of the connection to a host.
    /// </summary>
    /// <param name="host">The host.</param>
    /// <returns>The identity.</returns>
    public static RepositoryConnectionId For(RepositoryHost host) => new(host.ToString());
}
