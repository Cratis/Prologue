// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Discover.Runtime;

/// <summary>
/// Matches findings in the code to what the running system did.
/// </summary>
public static class RuntimeMatching
{
    /// <summary>
    /// Gets how often a capability was seen at runtime: the captures of every command shape that reaches it.
    /// </summary>
    /// <param name="shapes">The command shapes of the capability.</param>
    /// <param name="evidence">What was captured.</param>
    /// <returns>The number of captures; zero when the capability was never exercised or no HTTP command reaches it.</returns>
    public static int Seen(IEnumerable<RouteShape> shapes, RuntimeEvidence evidence) =>
        shapes.Select(evidence.Find).OfType<RuntimeShapeActivity>().Sum(_ => _.Total);

    /// <summary>
    /// Matches a decision point to the evidence of its kind: refusals count the command being refused, state changes count it
    /// being accepted, and a schedule counts the transactions no command explains.
    /// </summary>
    /// <param name="kind">What the decision point does.</param>
    /// <param name="shape">The command shape it belongs to.</param>
    /// <param name="evidence">What was captured.</param>
    /// <returns>How often it happened, which status it happened with most, and whether the evidence matches it.</returns>
    public static DecisionMatch Match(DecisionKind kind, RouteShape shape, RuntimeEvidence evidence)
    {
        if (kind == DecisionKind.Schedule)
        {
            return evidence.UncorrelatedTransactions > 0
                ? new(evidence.UncorrelatedTransactions, 0, DecisionRuntime.ExplainsWrites)
                : new(0, 0, DecisionRuntime.CodeOnly);
        }

        var activity = evidence.Find(shape);
        if (activity is null)
        {
            return new(0, 0, DecisionRuntime.CodeOnly);
        }

        var relevant = kind == DecisionKind.Refusal ? activity.Statuses.Where(_ => _.Status >= 400) : activity.Statuses.Where(_ => _.Status < 400);
        var counts = relevant.ToList();
        var count = counts.Sum(_ => _.Count);
        var status = counts.OrderByDescending(_ => _.Count).FirstOrDefault()?.Status ?? 0;
        return count > 0 ? new(count, status, DecisionRuntime.Matches) : new(0, 0, DecisionRuntime.CodeOnly);
    }
}
