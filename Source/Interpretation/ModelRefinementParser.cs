// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Prologue.Interpretation;

/// <summary>
/// Parses the language model's response text into a <see cref="ModelRefinement"/> — defensively, because models
/// wrap JSON in prose, truncate output, and invent shapes. The first balanced <c>{…}</c> object in the text is
/// deserialized; anything unusable yields <see langword="null"/> so the caller falls back to the unrefined model
/// instead of failing.
/// </summary>
public static class ModelRefinementParser
{
    static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Parses a language-model response into the refinement it describes.
    /// </summary>
    /// <param name="text">The raw response text.</param>
    /// <returns>The parsed <see cref="ModelRefinement"/>, or <see langword="null"/> when the text holds no parsable refinement.</returns>
    public static ModelRefinement? Parse(string text)
    {
        var json = FirstBalancedObject(text);
        if (json is null)
        {
            return null;
        }

        try
        {
            var response = JsonSerializer.Deserialize<Response>(json, _json);
            return response is null ? null : ToRefinement(response);
        }
        catch (JsonException)
        {
            // The balanced braces held something that is not the expected JSON shape — treat it as no refinement
            // so the caller falls back to the unrefined model.
            return null;
        }
    }

    static ModelRefinement ToRefinement(Response response) => new(
        response.SystemName ?? string.Empty,
        response.Renames ?? [],
        response.Descriptions ?? [],
        [
            .. (response.Questions ?? [])
                .Where(question => !string.IsNullOrWhiteSpace(question.Prompt))
                .Select(question => new RefinementQuestion(
                    question.Prompt!,
                    question.Context ?? string.Empty,
                    [
                        .. (question.Choices ?? [])
                            .Where(choice => !string.IsNullOrWhiteSpace(choice.Label))
                            .Select(choice => new QuestionChoice(choice.Label!, choice.Description ?? string.Empty))
                    ]))
        ]);

    static string? FirstBalancedObject(string text)
    {
        var start = text.IndexOf('{');
        if (start < 0)
        {
            return null;
        }

        var depth = 0;
        var inString = false;
        var escaped = false;
        for (var index = start; index < text.Length; index++)
        {
            var character = text[index];
            if (inString)
            {
                if (escaped)
                {
                    escaped = false;
                }
                else if (character == '\\')
                {
                    escaped = true;
                }
                else if (character == '"')
                {
                    inString = false;
                }

                continue;
            }

            switch (character)
            {
                case '"':
                    inString = true;
                    break;
                case '{':
                    depth++;
                    break;
                case '}':
                    depth--;
                    if (depth == 0)
                    {
                        return text[start..(index + 1)];
                    }

                    break;
            }
        }

        return null;
    }

    sealed record Response(string? SystemName, Dictionary<string, string>? Renames, Dictionary<string, string>? Descriptions, List<ResponseQuestion>? Questions);

    sealed record ResponseQuestion(string? Prompt, string? Context, List<ResponseChoice>? Choices);

    sealed record ResponseChoice(string? Label, string? Description);
}
