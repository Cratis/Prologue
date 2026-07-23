// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Cratis.Prologue.Configuration;
using Cratis.Prologue.Contracts;
using Cratis.Prologue.Interpreter.Contracts;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

namespace Cratis.Prologue.Interpretation;

/// <summary>
/// Represents an <see cref="IRefineExtraction"/> that asks a language model to refine the mechanical names in a
/// heuristically-built event model. The model only returns a name-to-name map, so the deterministic structure is
/// always preserved; any failure, timeout, or empty response falls back to the unrefined model.
/// </summary>
/// <param name="chatClient">The chat client used to reach the language model.</param>
/// <param name="options">The language-model options carrying the refinement timeout budget.</param>
/// <param name="logger">The logger.</param>
public sealed class LlmExtractionRefiner(IChatClient chatClient, IOptions<LlmOptions> options, ILogger<LlmExtractionRefiner> logger) : IRefineExtraction
{
    static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    /// <inheritdoc/>
    public async Task<ExtractionResult> Refine(ExtractionResult model, IReadOnlyList<Capture> captures, CancellationToken cancellationToken = default)
    {
        var names = NameCollector.Collect(model);
        if (names.Count == 0)
        {
            return model;
        }

        // Bound refinement to its own budget so a slow or hung model falls back to the heuristic model instead of
        // blocking extraction past the grain-call timeout. A cancellation from the caller's token is a real cancel;
        // one from our budget is a graceful fallback.
        using var budget = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        budget.CancelAfter(options.Value.RefinementTimeout);

        try
        {
            var response = await chatClient.GetResponseAsync(
                ExtractionPrompt.Build(names, captures),
                new ChatOptions { Temperature = 0.2f, ModelId = LlmChatClient.EffectiveModelId(options.Value) },
                budget.Token);

            var renames = ParseRenames(response.Text);
            return renames.Count == 0 ? model : ModelRenamer.Apply(model, renames);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            LlmExtractionRefinerLog.RefinementFailed(logger, exception);
            return model;
        }
    }

    static Dictionary<string, string> ParseRenames(string text)
    {
        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');
        if (start < 0 || end <= start)
        {
            return [];
        }

        var json = text[start..(end + 1)];
        return JsonSerializer.Deserialize<Dictionary<string, string>>(json, _json) ?? [];
    }
}
