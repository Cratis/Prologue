// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Decisions;
using Cratis.Prologue.Workbench.Discover.Agents;
using Cratis.Prologue.Workbench.Discover.Findings;
using Cratis.Prologue.Workbench.Discover.Running;
using Cratis.Prologue.Workbench.Discover.Runtime;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cratis.Prologue.Workbench.Discover;

/// <summary>
/// Registers Discover: the analysis queue and worker, the agents and the seam to runtime evidence.
/// </summary>
public static class DiscoverServiceCollectionExtensions
{
    /// <summary>
    /// Adds the analysis pipeline. <see cref="IRuntimeShapes"/> defaults to no evidence; register another to feed Discover from captures.
    /// </summary>
    /// <param name="builder">The <see cref="WebApplicationBuilder"/> to add to.</param>
    /// <returns>The builder for continuation.</returns>
    public static WebApplicationBuilder AddDiscover(this WebApplicationBuilder builder)
    {
        builder.Services.TryAddSingleton<IRuntimeShapes, NoRuntimeShapes>();

        // What Discover found in the code explains the decisions Capture shows, replacing the default of no reasons.
        builder.Services.Replace(ServiceDescriptor.Scoped<IDecisionReasons, FindingsDecisionReasons>());
        builder.Services.AddSingleton<IAnalysisProgress, AnalysisProgressStore>();
        builder.Services.AddSingleton<IAnalysisQueue, AnalysisQueue>();
        builder.Services.AddSingleton<ISourceFolders, SourceFolders>();
        builder.Services.AddSingleton<IAnalysisAgents, CratisAIAnalysisAgents>();
        builder.Services.AddSingleton<IAnalysisRunner, AnalysisRunner>();
        builder.Services.AddHostedService<AnalysisWorker>();
        return builder;
    }
}
