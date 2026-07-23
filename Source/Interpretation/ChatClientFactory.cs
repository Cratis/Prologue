// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Configuration;
using Microsoft.Extensions.AI;

namespace Cratis.Prologue.Interpretation;

/// <summary>
/// Represents an <see cref="IChatClientFactory"/> that creates the real provider clients through
/// <see cref="LlmChatClient"/> — Ollama, OpenAI, Azure OpenAI, OpenAI-compatible endpoints, and Anthropic.
/// </summary>
public class ChatClientFactory : IChatClientFactory
{
    /// <inheritdoc/>
    public IChatClient CreateFor(LlmOptions options) => LlmChatClient.CreateFor(options);
}
