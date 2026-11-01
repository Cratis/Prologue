// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Captures.Live;
using Cratis.Prologue.Workbench.Explore.Matrix;

namespace Cratis.Prologue.Workbench.Discover.Runtime;

/// <summary>
/// Works out the runtime evidence from what the captures have been reduced to.
/// </summary>
public static class CapturedRuntimeEvidence
{
    /// <summary>
    /// Builds the evidence: the status counts of every captured command shape from the command matrix, and the database
    /// transactions no command explains from the capture aggregates. Either may be missing while nothing was captured.
    /// </summary>
    /// <param name="matrix">The command matrix of the prologue, or null when nothing was captured.</param>
    /// <param name="aggregates">The capture aggregates of the prologue, or null when nothing was captured.</param>
    /// <returns>The evidence.</returns>
    public static RuntimeEvidence From(CommandMatrix? matrix, CaptureAggregates? aggregates) =>
        new(
            [
                .. (matrix?.Shapes ?? [])
                    .Select(profile => new RuntimeShapeActivity(
                        new RouteShape(profile.Shape.Value),
                        [.. profile.Statuses.Select(status => new StatusCount(status.Status.Value, status.Captures))]))
            ],
            aggregates?.UncorrelatedTransactions ?? 0);
}
