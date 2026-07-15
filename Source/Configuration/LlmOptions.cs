// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Configuration;

/// <summary>
/// Represents the configuration for the language model the interpreter uses to refine the extracted event model's
/// names, bound from the <c>Llm</c> configuration section. Any supported <see cref="LlmKind"/> works — the default
/// points at the local Ollama service bundled with the Studio docker-compose stack.
/// </summary>
public class LlmOptions
{
    /// <summary>
    /// The configuration section name the options are bound from.
    /// </summary>
    public const string SectionName = "Llm";

    /// <summary>
    /// Gets or sets a value indicating whether language-model refinement is enabled. When disabled, the interpreter
    /// returns the deterministic heuristic model unchanged.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the kind of language-model provider to use.
    /// </summary>
    public LlmKind Kind { get; set; } = LlmKind.Ollama;

    /// <summary>
    /// Gets or sets the base endpoint of the language-model service. Optional for the hosted providers
    /// (<see cref="LlmKind.OpenAI"/> and <see cref="LlmKind.Anthropic"/>), which default to their public endpoints.
    /// </summary>
    public string Endpoint { get; set; } = "http://llm:11434";

    /// <summary>
    /// Gets or sets the access token (API key) sent to the language-model service. Ignored by Ollama.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifier of the model to use for refinement. For <see cref="LlmKind.AzureOpenAI"/> this
    /// is the deployment name. When empty, the hosted providers fall back to their default model.
    /// </summary>
    public string ModelId { get; set; } = "gemma4:e2b-it-q4_K_M";

    /// <summary>
    /// Gets or sets how long refinement may wait for the language model before falling back to the unrefined
    /// heuristic model. Refinement only improves names, so a slow or unavailable model must never block extraction.
    /// </summary>
    public TimeSpan RefinementTimeout { get; set; } = TimeSpan.FromMinutes(2);
}
