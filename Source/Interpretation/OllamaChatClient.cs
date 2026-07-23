// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;

namespace Cratis.Prologue.Interpretation;

/// <summary>
/// A minimal <see cref="IChatClient"/> that calls Ollama's native <c>/api/chat</c> endpoint directly. This avoids
/// routing through the OpenAI SDK, which throws on <c>finish_reason</c> values it does not recognize when Ollama
/// is accessed through its OpenAI-compatible endpoint.
/// </summary>
internal sealed class OllamaChatClient : IChatClient
{
    // Local models can take several minutes to refine a large model, so allow a very generous timeout
    // (the default HttpClient timeout of 100 seconds cancels the request mid-generation).
    static readonly HttpClient _http = new() { Timeout = TimeSpan.FromMinutes(30) };

    static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    readonly Uri _chatUri;
    readonly string _modelName;

    internal OllamaChatClient(string endpoint, string modelName)
    {
        _modelName = modelName;
        _chatUri = new Uri(endpoint.TrimEnd('/') + "/api/chat");
    }

    /// <inheritdoc/>
    public void Dispose()
    {
    }

    /// <inheritdoc/>
    public object? GetService(Type serviceType, object? serviceKey = null) =>
        serviceType.IsInstanceOfType(this) ? this : null;

    /// <inheritdoc/>
    public async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var modelId = options?.ModelId ?? _modelName;
        var request = new OllamaRequest(
            modelId,
            [.. messages.Select(message => new OllamaMessage(message.Role.Value, message.Text ?? string.Empty))],
            Stream: false,
            Options: options?.Temperature is { } temperature ? new OllamaModelOptions(temperature) : null);

        var httpResponse = await _http.PostAsJsonAsync(_chatUri, request, _jsonOptions, cancellationToken);

        // Ollama does not auto-download models; download on demand and retry once so refinement works without a
        // manual `ollama pull` setup step.
        if (httpResponse.StatusCode == HttpStatusCode.NotFound)
        {
            var detail = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            if (detail.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                httpResponse.Dispose();
                await PullModel(modelId, cancellationToken);
                httpResponse = await _http.PostAsJsonAsync(_chatUri, request, _jsonOptions, cancellationToken);
            }
        }

        httpResponse.EnsureSuccessStatusCode();

        var body = await httpResponse.Content.ReadFromJsonAsync<OllamaResponse>(_jsonOptions, cancellationToken)
            ?? throw new EmptyLlmResponse(_chatUri);

        return new ChatResponse([new ChatMessage(ChatRole.Assistant, body.Message?.Content ?? string.Empty)])
        {
            FinishReason = ChatFinishReason.Stop,
            ModelId = body.Model ?? _modelName,
        };
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var full = await GetResponseAsync(messages, options, cancellationToken);
        yield return new ChatResponseUpdate(ChatRole.Assistant, full.Text)
        {
            FinishReason = full.FinishReason,
            ModelId = full.ModelId,
        };
    }

    async Task PullModel(string modelId, CancellationToken cancellationToken)
    {
        var pullUri = new Uri(_chatUri, "/api/pull");

        // Best effort — a model that is already present may respond with a non-success status; the retry that
        // follows surfaces any genuine failure.
        using var response = await _http.PostAsJsonAsync(pullUri, new OllamaPullRequest(modelId, Stream: false), _jsonOptions, cancellationToken);
    }

    sealed record OllamaRequest(string Model, List<OllamaMessage> Messages, bool Stream, OllamaModelOptions? Options);

    sealed record OllamaMessage(string Role, string Content);

    sealed record OllamaModelOptions(float Temperature);

    sealed record OllamaResponse(string? Model, OllamaMessage? Message, bool Done, string? DoneReason);

    sealed record OllamaPullRequest(string Model, bool Stream);
}
