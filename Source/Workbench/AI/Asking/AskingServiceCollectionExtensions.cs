// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.AI.LanguageModels;
using Cratis.AI.Providers;
using Cratis.AI.Usage.Daily;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cratis.Prologue.Workbench.AI.Asking;

/// <summary>
/// Registers what lets the Workbench ask its agents a question.
/// </summary>
public static class AskingServiceCollectionExtensions
{
    /// <summary>
    /// Adds the <see cref="IAgentAsker"/> and, unless one is already registered, the Cratis.AI language model it asks through:
    /// every completion resolves to the provider or pool configured on the calling agent and fails when it names none.
    /// </summary>
    /// <param name="builder">The <see cref="WebApplicationBuilder"/> to add to.</param>
    /// <returns>The builder for continuation.</returns>
    public static WebApplicationBuilder AddAsking(this WebApplicationBuilder builder)
    {
        // Every IAIProviderClient takes the untyped IHttpClientFactory registration; without it each vendor call fails at runtime.
        builder.Services.AddHttpClient(Microsoft.Extensions.Options.Options.DefaultName)
            .ConfigureHttpClient(client => client.Timeout = TimeSpan.FromSeconds(150));
        builder.Services.TryAddTransient<ILanguageModel>(services => ActivatorUtilities.CreateInstance<ManagedLanguageModel>(
            services,
            ActivatorUtilities.CreateInstance<ProviderAwareLanguageModel>(services)));
        builder.Services.TryAddScoped<IRecordedAgentSessions, NoRecordedAgentSessions>();
        builder.Services.TryAddScoped<IAgentAsker, LanguageModelAsker>();
        return builder;
    }
}
