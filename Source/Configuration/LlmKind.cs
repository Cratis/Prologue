// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Configuration;

/// <summary>
/// Represents the kind of language-model provider the interpreter talks to.
/// </summary>
public enum LlmKind
{
    /// <summary>
    /// An Ollama service, reached through its native chat API.
    /// </summary>
    Ollama,

    /// <summary>
    /// OpenAI's hosted API.
    /// </summary>
    OpenAI,

    /// <summary>
    /// An Azure OpenAI deployment — the model identifier is the deployment name.
    /// </summary>
    AzureOpenAI,

    /// <summary>
    /// Any service exposing an OpenAI-compatible <c>/v1</c> API.
    /// </summary>
    OpenAICompatible,

    /// <summary>
    /// Anthropic's hosted API.
    /// </summary>
    Anthropic
}
