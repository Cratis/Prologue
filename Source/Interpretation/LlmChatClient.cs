// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ClientModel;
using Anthropic;
using Anthropic.Core;
using Cratis.Prologue.Configuration;
using Microsoft.Extensions.AI;

namespace Cratis.Prologue.Interpretation;

/// <summary>
/// Creates the <see cref="IChatClient"/> for the configured language-model provider — the same provider set
/// Studio supports: Ollama, OpenAI, Azure OpenAI, any OpenAI-compatible endpoint, and Anthropic.
/// </summary>
public static class LlmChatClient
{
    /// <summary>
    /// The model used for <see cref="LlmKind.OpenAI"/> when no model is configured.
    /// </summary>
    public const string DefaultOpenAIModelId = "gpt-4o-mini";

    /// <summary>
    /// The model used for <see cref="LlmKind.Anthropic"/> when no model is configured.
    /// </summary>
    public const string DefaultAnthropicModelId = "claude-opus-4-6";

    /// <summary>
    /// Creates the chat client for the configured provider.
    /// </summary>
    /// <param name="options">The language-model options.</param>
    /// <returns>The <see cref="IChatClient"/> reaching the configured provider.</returns>
    public static IChatClient CreateFor(LlmOptions options) => options.Kind switch
    {
        LlmKind.OpenAI => new OpenAI.Chat.ChatClient(
            EffectiveModelId(options),
            new ApiKeyCredential(AccessTokenOrPlaceholder(options)))
            .AsIChatClient(),

        LlmKind.AzureOpenAI => new OpenAI.Chat.ChatClient(
            EffectiveModelId(options),
            new ApiKeyCredential(AccessTokenOrPlaceholder(options)),
            new OpenAI.OpenAIClientOptions { Endpoint = new Uri($"{options.Endpoint.TrimEnd('/')}/openai/v1/") })
            .AsIChatClient(),

        LlmKind.OpenAICompatible => new OpenAI.Chat.ChatClient(
            EffectiveModelId(options),
            new ApiKeyCredential(AccessTokenOrPlaceholder(options)),
            new OpenAI.OpenAIClientOptions { Endpoint = NormalizeOpenAICompatibleEndpoint(options.Endpoint) })
            .AsIChatClient(),

        LlmKind.Anthropic => CreateAnthropic(options),

        _ => new OllamaChatClient(options.Endpoint, EffectiveModelId(options))
    };

    /// <summary>
    /// Resolves the effective model identifier for the configured provider, falling back to the provider's
    /// default model when none is configured.
    /// </summary>
    /// <param name="options">The language-model options.</param>
    /// <returns>The model identifier to use.</returns>
    public static string EffectiveModelId(LlmOptions options) =>
        options.ModelId is { Length: > 0 } model
            ? model
            : options.Kind switch
            {
                LlmKind.OpenAI => DefaultOpenAIModelId,
                LlmKind.Anthropic => DefaultAnthropicModelId,
                _ => options.ModelId
            };

    static IChatClient CreateAnthropic(LlmOptions options)
    {
        var clientOptions = new ClientOptions { ApiKey = options.AccessToken };
        if (options.Endpoint is { Length: > 0 } endpoint && options.Kind == LlmKind.Anthropic && IsCustomAnthropicEndpoint(endpoint))
        {
            clientOptions.BaseUrl = endpoint;
        }

        return new AnthropicClient(clientOptions).AsIChatClient();
    }

    /// <summary>
    /// The default Ollama endpoint doubles as the option's default value; treat anything else as a deliberate
    /// override of Anthropic's public endpoint.
    /// </summary>
    /// <param name="endpoint">The configured endpoint.</param>
    /// <returns>Whether the endpoint deliberately overrides Anthropic's public endpoint.</returns>
    static bool IsCustomAnthropicEndpoint(string endpoint) =>
        !string.Equals(endpoint, "http://llm:11434", StringComparison.OrdinalIgnoreCase);

    static string AccessTokenOrPlaceholder(LlmOptions options) =>
        options.AccessToken is { Length: > 0 } token ? token : "not-needed";

    static Uri NormalizeOpenAICompatibleEndpoint(string endpoint)
    {
        var normalized = endpoint.TrimEnd('/');
        if (!normalized.EndsWith("/v1", StringComparison.OrdinalIgnoreCase))
        {
            normalized = $"{normalized}/v1";
        }

        return new Uri($"{normalized}/");
    }
}
