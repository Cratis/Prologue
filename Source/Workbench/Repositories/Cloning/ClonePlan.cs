// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Prologues;
using Cratis.Prologue.Workbench.Systems;

namespace Cratis.Prologue.Workbench.Repositories.Cloning;

/// <summary>
/// Plans a clone from a system's code location.
/// </summary>
public static class ClonePlan
{
    /// <summary>
    /// Plans the clone for a location in a prologue. Every system sharing the repository and branch contributes its sub path
    /// so one clone serves them all.
    /// </summary>
    /// <param name="prologue">The prologue.</param>
    /// <param name="location">The location of the system to clone.</param>
    /// <param name="connection">The connection to the location's host, when there is one.</param>
    /// <returns>The request.</returns>
    public static CloneRequest For(PrologueDefinition prologue, SystemCodeLocation location, RepositoryConnection? connection)
    {
        var shared = prologue.Systems
            .Select(system => system.Location)
            .OfType<SystemCodeLocation>()
            .Where(other => other.Host == location.Host && other.Repository == location.Repository && other.Branch == location.Branch)
            .Select(other => other.SubPath)
            .ToList();

        return new CloneRequest(location.Host, location.Repository, location.Branch, location.SubPath, shared, connection?.Url, connection?.Secret, connection?.Installation, connection?.App);
    }
}
