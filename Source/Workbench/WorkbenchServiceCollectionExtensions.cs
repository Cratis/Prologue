// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.AI.Configuration;
using Cratis.Prologue.Workbench.AI;
using Cratis.Prologue.Workbench.Identity;
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
            configureArcBuilder: arc => arc.WithMongoDB(),
            configureChronicleOptions: options =>
            {
                options.EventStore = EventStore;
                options.ProgramIdentifier = "Cratis Prologue";
                options.ConnectTimeout = 30;
            },
            configureChronicleBuilder: chronicle => chronicle.WithCamelCaseNamingPolicy().WithIdentityProvider(identityProvider));

        builder.AddRepositories();

        return builder;
    }
}
