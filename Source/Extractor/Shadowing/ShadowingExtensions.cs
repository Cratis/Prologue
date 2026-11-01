// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Configuration;
using Yarp.ReverseProxy.Configuration;

namespace Cratis.Prologue.Extractor.Shadowing;

/// <summary>
/// Registers shadowing.
/// </summary>
public static class ShadowingExtensions
{
    /// <summary>
    /// The name of the HTTP client that sends the copies to the shadow destination.
    /// </summary>
    public const string ShadowClientName = "prologue-shadow";

    /// <summary>
    /// Adds shadowing when the configuration names commands to shadow. Parity is reported to the Receiver, so the Prologue the captures
    /// belong to and the Receiver as output are required.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="options">The Prologue options.</param>
    /// <returns>The <see cref="ShadowRouting"/> built from the configuration; <see cref="ShadowRouting.Any"/> is false when nothing is shadowed.</returns>
    /// <exception cref="InvalidOperationException">Shadowing is configured without a Prologue or without the Receiver as output.</exception>
    public static ShadowRouting AddShadowing(this IServiceCollection services, PrologueOptions options)
    {
        var routing = new ShadowRouting(options.Shadowing);
        if (!routing.Any)
        {
            return routing;
        }

        if (options.PrologueId == Guid.Empty || options.Output.Kind != OutputKind.Api)
        {
            throw new InvalidOperationException("Shadowing reports parity to the Receiver: set prologue.prologueId and use the Api output.");
        }

        services.AddSingleton(routing);
        services.AddHttpClient(ShadowClientName).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false });
        services.AddHttpClient<IParityReporter, ParityReporter>((client, provider) =>
        {
            client.BaseAddress = new Uri(options.Output.Api.Endpoint.TrimEnd('/') + "/");
            return new ParityReporter(client, options.PrologueId, provider.GetRequiredService<ILogger<ParityReporter>>());
        });
        services.AddSingleton<IShadowSender>(provider => new ShadowSender(
            provider.GetRequiredService<IHttpClientFactory>().CreateClient(ShadowClientName),
            provider.GetRequiredService<ILogger<ShadowSender>>()));
        services.AddSingleton(provider => new ShadowDispatcher(
            provider.GetRequiredService<IShadowSender>(),
            provider.GetRequiredService<IParityReporter>(),
            provider.GetRequiredService<ILogger<ShadowDispatcher>>(),
            options.ShadowQueueBytes));
        services.AddSingleton<IShadowQueue>(provider => provider.GetRequiredService<ShadowDispatcher>());
        services.AddHostedService(provider => provider.GetRequiredService<ShadowDispatcher>());
        return routing;
    }
}
