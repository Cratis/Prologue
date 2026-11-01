// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Interpretation;
using Cratis.Prologue.Workbench.Events.Interpreting;
using Cratis.Prologue.Workbench.Events.Questions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Prologue.Workbench.Events;

/// <summary>
/// Registers the Events step.
/// </summary>
public static class EventsServiceCollectionExtensions
{
    /// <summary>
    /// Adds the interpretation pipeline: the Interpretation library's heuristics and sessions, backed by Cratis.AI, run in the background.
    /// </summary>
    /// <param name="builder">The <see cref="WebApplicationBuilder"/> to add to.</param>
    /// <returns>The builder for continuation.</returns>
    public static WebApplicationBuilder AddEvents(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IInterpretationProgress, InterpretationProgressStore>();
        builder.Services.AddSingleton<IInterpretationQueue, InterpretationQueue>();
        builder.Services.AddSingleton<IInterpretationSessions, InterpretationSessions>();
        builder.Services.AddSingleton<IBuildHeuristicModel, HeuristicModelBuilder>();
        builder.Services.AddScoped<IChatClientFactory, CratisAIChatClientFactory>();
        builder.Services.AddScoped<IEvidenceSource, WorkbenchEvidenceSource>();
        builder.Services.AddScoped<IInterpretationRunner, InterpretationRunner>();
        builder.Services.AddHostedService<InterpretationWorker>();
        return builder;
    }
}
