// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Discovery.Runtime;

/// <summary>
/// What the running system did for one command shape, as captured.
/// </summary>
/// <param name="Shape">The command shape, in the format captures use: "METHOD /path" with identifying segments as {id}.</param>
/// <param name="Statuses">How many times it ended in each HTTP status.</param>
public record RuntimeShapeActivity(RouteShape Shape, IEnumerable<StatusCount> Statuses)
{
    /// <summary>
    /// Gets how many times the command was captured.
    /// </summary>
    public int Total => Statuses.Sum(_ => _.Count);

    /// <summary>
    /// Gets how many times the command was refused, which is any client or server error.
    /// </summary>
    public int Refused => Statuses.Where(_ => _.Status >= 400).Sum(_ => _.Count);

    /// <summary>
    /// Gets how many times the command was accepted.
    /// </summary>
    public int Accepted => Total - Refused;
}
