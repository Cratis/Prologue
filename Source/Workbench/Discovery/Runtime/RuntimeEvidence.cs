// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Discovery.Runtime;

/// <summary>
/// What was captured at runtime for a prologue, which is what findings in the code are matched against.
/// </summary>
/// <param name="Shapes">The activity of each command shape that was captured.</param>
/// <param name="UncorrelatedTransactions">The number of database transactions that no HTTP command explains, which a scheduled job in the code can explain.</param>
public record RuntimeEvidence(IEnumerable<RuntimeShapeActivity> Shapes, int UncorrelatedTransactions)
{
    /// <summary>
    /// Gets evidence with nothing captured.
    /// </summary>
    public static readonly RuntimeEvidence None = new([], 0);

    /// <summary>
    /// Gets the activity of a command shape, ignoring case.
    /// </summary>
    /// <param name="shape">The shape.</param>
    /// <returns>The activity, or null when it was never captured.</returns>
    public RuntimeShapeActivity? Find(RouteShape shape) =>
        Shapes.FirstOrDefault(_ => string.Equals(_.Shape.Value, shape.Value, StringComparison.OrdinalIgnoreCase));
}
