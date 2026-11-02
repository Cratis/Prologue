// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.AI.Usage.Daily;
using Cratis.Prologue.Workbench.AI.Dispatch;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cratis.Prologue.Workbench.AI.Asking;

/// <summary>
/// Registers what lets the Workbench ask its agents a question.
/// </summary>
public static class AskingServiceCollectionExtensions
{
    /// <summary>
    /// Adds the <see cref="IAgentAsker"/>, which asks through the dispatching chat clients. No language model service is registered: nothing in the Workbench
    /// or in Cratis.AI's constructors takes one, and a registered one would be a way around the dispatcher's order, pause and ceiling.
    /// </summary>
    /// <param name="builder">The <see cref="WebApplicationBuilder"/> to add to.</param>
    /// <returns>The builder for continuation.</returns>
    public static WebApplicationBuilder AddAsking(this WebApplicationBuilder builder)
    {
        // Every IAIProviderClient takes the untyped IHttpClientFactory registration; without it each vendor call fails at runtime.
        builder.Services.AddHttpClient(Microsoft.Extensions.Options.Options.DefaultName)
            .ConfigureHttpClient(client => client.Timeout = TimeSpan.FromSeconds(150));
        builder.Services.TryAddScoped<IRecordedAgentSessions, Usage.RecordedAgentSessions>();
        builder.AddDispatch();
        builder.Services.TryAddScoped<IAgentAsker, LanguageModelAsker>();
        return builder;
    }
}
