// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Discovery.Agents;

namespace Cratis.Prologue.Workbench.Discovery.Running.for_AnalysisRunner;

/// <summary>
/// Agents that answer every batch with the same findings, or that cannot be reached.
/// </summary>
/// <param name="findings">What they find in each batch.</param>
/// <param name="failures">Why they cannot answer, when they cannot.</param>
public class FakeAgents(AnalysisFindings findings, params string[] failures) : IAnalysisAgents
{
    public List<AnalysisBatch> Batches { get; } = [];

    public Task<AgentFindings> Read(AnalysisBatch batch, CancellationToken cancellationToken)
    {
        Batches.Add(batch);
        return Task.FromResult(new AgentFindings(failures.Length == 0 ? findings : AnalysisFindings.Empty, failures));
    }
}
#endif
