// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace Cratis.Prologue.Workbench.Discover.Agents;

/// <summary>
/// Log messages for <see cref="CratisAIAnalysisAgents"/>.
/// </summary>
internal static partial class CratisAIAnalysisAgentsLogMessages
{
    [LoggerMessage(LogLevel.Warning, "The {Agent} could not answer: {Reason}")]
    internal static partial void AgentUnavailable(this ILogger<CratisAIAnalysisAgents> logger, string agent, string reason);

    [LoggerMessage(LogLevel.Information, "Dropped an entry from the {Agent}'s answer: {Reason}")]
    internal static partial void EntryDropped(this ILogger<CratisAIAnalysisAgents> logger, string agent, string reason);
}
