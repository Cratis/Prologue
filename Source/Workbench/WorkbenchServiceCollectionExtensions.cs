// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.AI.Configuration;
using Cratis.AI.Usage.Daily;
using Cratis.Chronicle.Connections;
using Cratis.Prologue.Workbench.AI;
using Cratis.Prologue.Workbench.AI.Asking;
using Cratis.Prologue.Workbench.Captures;
using Cratis.Prologue.Workbench.Discover;
using Cratis.Prologue.Workbench.Identity;
using Cratis.Prologue.Workbench.Rebuild;
using Cratis.Prologue.Workbench.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Prologue.Workbench;

/// <summary>
/// Registers the Workbench's Arc, Chronicle and Cratis.AI hosting.
/// </summary>
public static class WorkbenchServiceCollectionExtensions
{
    /// <summary>
    /// The name of the Chronicle event store holding the Workbench's events.
    /// </summary>
    public const string EventStore = "Prologue";

    /// <summary>
    /// Adds the Workbench: Cratis.AI first, then Arc with Chronicle and MongoDB read models.
    /// </summary>
    /// <param name="builder">The <see cref="WebApplicationBuilder"/> to add to.</param>
    /// <returns>The builder for continuation.</returns>
    public static WebApplicationBuilder AddWorkbench(this WebApplicationBuilder builder)
    {
        // Chronicle captures its identity provider while the client is built, and Cratis.AI needs it resolvable from DI.
        var identityProvider = new WorkbenchIdentityProvider();
        builder.Services.AddSingleton<Cratis.Chronicle.Identities.IIdentityProvider>(identityProvider);

        // Must run before AddCratis(): Arc snapshots its type universe the first time it is touched.
        builder.Services.AddCratisAI(ai => ai.WithAgents<PrologueAIAgents>());

        builder.AddCratis(
            options =>
            {
                // The generated proxies (Workbench.csproj, CratisProxies*) build their routes the same way: /api/<feature>/<slice>.
                options.GeneratedApis.RoutePrefix = "api";
                options.GeneratedApis.IncludeCommandNameInRoute = false;
                options.GeneratedApis.SegmentsToSkipForRoute = 1;
            },
            configureArcBuilder: arc => arc.WithMongoDB(),
            configureChronicleOptions: options =>
            {
                options.EventStore = EventStore;
                options.ProgramIdentifier = "Cratis Prologue";
                options.ConnectTimeout = 30;
            },
            configureChronicleBuilder: chronicle => chronicle.WithCamelCaseNamingPolicy().WithIdentityProvider(identityProvider));

        // Arc's read protection and the scoped IEventStore resolve the event store from the client options, which the
        // configureChronicleOptions callback above does not reach: without this they ask for an event store with no name.
        builder.Services.PostConfigure<ChronicleClientOptions>(options => options.EventStore = EventStore);

        builder.AddRepositories();
        builder.AddCaptures();
        builder.AddAsking();
        builder.AddDiscover();
        builder.AddRebuild();

        return builder;
    }
}
