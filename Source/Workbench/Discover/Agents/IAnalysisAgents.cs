// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Discover.Agents;

/// <summary>
/// Asks the Cratis.AI agents to read a batch of source files. Every call goes through Cratis.AI; nothing here talks to a vendor.
/// </summary>
public interface IAnalysisAgents
{
    /// <summary>
    /// Has the Dramaturg, the Prompter and the Usher read a batch.
    /// </summary>
    /// <param name="batch">The files.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>What they found, and why any of them could not answer.</returns>
    Task<AgentFindings> Read(AnalysisBatch batch, CancellationToken cancellationToken);
}

/// <summary>
/// What the agents found in a batch.
/// </summary>
/// <param name="Findings">The valid findings.</param>
/// <param name="Failures">Why an agent could not answer, such as no provider being configured; empty when all three did.</param>
public record AgentFindings(AnalysisFindings Findings, IReadOnlyList<string> Failures);
