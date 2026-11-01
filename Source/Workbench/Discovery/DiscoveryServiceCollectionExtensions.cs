// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Discovery.Agents;
using Cratis.Prologue.Workbench.Discovery.Running;
using Cratis.Prologue.Workbench.Discovery.Runtime;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cratis.Prologue.Workbench.Discovery;

/// <summary>
/// Registers Discover: the analysis queue and worker, the agents and the seam to runtime evidence.
/// </summary>
public static class DiscoveryServiceCollectionExtensions
{
    /// <summary>
    /// Adds the analysis pipeline. <see cref="IRuntimeShapes"/> defaults to no evidence; register another to feed Discover from captures.
    /// </summary>
    /// <param name="builder">The <see cref="WebApplicationBuilder"/> to add to.</param>
    /// <returns>The builder for continuation.</returns>
    public static WebApplicationBuilder AddDiscovery(this WebApplicationBuilder builder)
    {
        builder.Services.TryAddSingleton<IRuntimeShapes, NoRuntimeShapes>();
        builder.Services.AddSingleton<IAnalysisProgress, AnalysisProgressStore>();
        builder.Services.AddSingleton<IAnalysisQueue, AnalysisQueue>();
        builder.Services.AddSingleton<ISourceFolders, SourceFolders>();
        builder.Services.AddSingleton<IAnalysisAgents, CratisAIAnalysisAgents>();
        builder.Services.AddSingleton<IAnalysisRunner, AnalysisRunner>();
        builder.Services.AddHostedService<AnalysisWorker>();
        return builder;
    }
}
