// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Captures;

namespace Cratis.Prologue.Workbench.Explore.Journeys;

/// <summary>
/// Finds the identities in a request path, the only link between captures that Prologue can see: it never reads a row value.
/// </summary>
public static class PathIdentities
{
    /// <summary>
    /// Finds the identities in a path. A segment is an identity by the same rule that makes it "{id}" in a command shape, and what
    /// it identifies is the segment in front of it.
    /// </summary>
    /// <param name="path">The recorded request path.</param>
    /// <returns>The identities in the order they appear.</returns>
    public static IEnumerable<PathIdentity> From(RequestPath path)
    {
        var kind = EntityKind.Unnamed;
        foreach (var segment in path.Value.Split('?', '#')[0].Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            if (CommandShape.IsIdentifier(segment))
            {
                yield return new(kind, new EntityId(segment));
            }
            else
            {
                kind = new EntityKind(segment.ToLowerInvariant());
            }
        }
    }
}
