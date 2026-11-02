// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.AI.Conversations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cratis.Prologue.Workbench.AI.Dispatch;

/// <summary>
/// Registers the dispatch that puts the Workbench's pool arrangements in front of Cratis.AI's chat clients.
/// </summary>
public static class DispatchServiceCollectionExtensions
{
    /// <summary>
    /// Replaces <see cref="IAIChatClients"/> with <see cref="DispatchingAIChatClients"/>, wrapping Cratis.AI's own implementation under its original lifetime,
    /// so every caller that asks for an agent's chat client is dispatched by the recorded order, fallback and ceiling without changing.
    /// </summary>
    /// <param name="builder">The <see cref="WebApplicationBuilder"/> to add to.</param>
    /// <returns>The builder for continuation.</returns>
    public static WebApplicationBuilder AddDispatch(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var lifetime = services.LastOrDefault(_ => _.ServiceType == typeof(IAIChatClients))?.Lifetime ?? ServiceLifetime.Scoped;
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddScoped<IPoolPlans, PoolPlans>();
        services.TryAddScoped<IPoolSpendRecorder, PoolSpendRecorder>();
        services.Replace(new ServiceDescriptor(
            typeof(IAIChatClients),
            provider => new DispatchingAIChatClients(
                ActivatorUtilities.CreateInstance<AIChatClients>(provider),
                provider.GetRequiredService<IPoolPlans>(),
                provider.GetRequiredService<IPoolSpendRecorder>()),
            lifetime == ServiceLifetime.Singleton ? ServiceLifetime.Scoped : lifetime));
        return builder;
    }
}
