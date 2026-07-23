// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using System.Text;
using System.Text.Json;
using Cratis.Prologue.Contracts;
using Cratis.Prologue.Interpreter.Contracts;

namespace Cratis.Prologue.Interpretation;

/// <summary>
/// Builds the messages of the structured refinement conversation — the system instructions (from the embedded
/// <c>Prompts/refinement.txt</c> resource, with the question policy switched off for non-interactive hosts), the
/// user evidence message carrying the provisional model and the observed behavior and schema, and the follow-up
/// message that relays the user's answers when the model asked questions.
/// </summary>
public static class RefinementPrompt
{
    const string ResourceName = "Cratis.Prologue.Interpretation.Prompts.refinement.txt";

    const string QuestionPolicy = """
        Questions:
        - Ask a question ONLY when you are genuinely uncertain about a decision that materially changes the model — zero questions is the normal case.
        - Keep each question about exactly one decision.
        - Prefer multiple choice with 2 to 4 options; give each choice a short label and, when helpful, a one-line description.
        - Never add an "Other" choice — the user always has a free-text "other" option on top of your choices.
        - When you have no questions, return an empty "questions" array.
        """;

    const string NoQuestionsPolicy = """
        Questions:
        - Do not ask questions. Always return an empty "questions" array and decide everything with your best judgment from the evidence.
        """;

    static readonly string _template = LoadTemplate();

    /// <summary>
    /// Builds the system message that instructs the model how to refine and how to respond.
    /// </summary>
    /// <param name="allowQuestions">Whether the model may ask the user questions; non-interactive hosts disallow them so a run can never stall.</param>
    /// <returns>The system message text.</returns>
    public static string System(bool allowQuestions) =>
        _template.Replace("{QUESTION_POLICY}", allowQuestions ? QuestionPolicy : NoQuestionsPolicy);

    /// <summary>
    /// Builds the user evidence message — the provisional model outline, the provisional names the renames must
    /// key on, the observed behavior, and the observed database schema when one was captured.
    /// </summary>
    /// <param name="model">The provisional event model to refine.</param>
    /// <param name="captures">The captures the model was derived from.</param>
    /// <returns>The evidence message text.</returns>
    public static string Evidence(ExtractionResult model, IReadOnlyList<Capture> captures)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Provisional event model (structure is fixed; only names, descriptions, and the system name are yours to decide):");
        AppendOutline(builder, model);
        builder
            .AppendLine()
            .AppendLine("Provisional names (every key in \"renames\" must be one of these):")
            .AppendLine(JsonSerializer.Serialize(NameCollector.Collect(model)))
            .AppendLine()
            .AppendLine("Observed behavior (evidence):")
            .Append(EvidenceFormatter.ObservedBehavior(captures));

        var schema = EvidenceFormatter.DatabaseSchema(captures);
        if (schema.Length > 0)
        {
            builder
                .AppendLine()
                .AppendLine("Observed database schema (column types, sizes, nullability, keys, unique constraints, relationships):")
                .Append(schema);
        }

        return builder.ToString();
    }

    /// <summary>
    /// Builds the follow-up user message that relays the answers to the model's questions.
    /// </summary>
    /// <param name="answers">The answered questions of the round being relayed.</param>
    /// <returns>The answers message text.</returns>
    public static string Answers(IReadOnlyList<AnsweredQuestion> answers)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Answers to your questions:");
        foreach (var answered in answers)
        {
            builder
                .AppendLine($"- Q: {answered.Question.Prompt}")
                .AppendLine($"  A: {answered.Answer}");
        }

        builder.AppendLine("Respond again with the same single JSON object shape, updated with what you now know.");
        return builder.ToString();
    }

    static void AppendOutline(StringBuilder builder, ExtractionResult model)
    {
        foreach (var module in model.Modules)
        {
            builder.AppendLine($"- module {module.Name}");
            foreach (var feature in module.Features)
            {
                AppendFeature(builder, feature, module.Name, "  ");
            }
        }
    }

    static void AppendFeature(StringBuilder builder, ExtractedFeature feature, string path, string indent)
    {
        var featurePath = $"{path}/{feature.Name}";
        builder.AppendLine($"{indent}- feature {featurePath}");
        foreach (var slice in feature.Slices)
        {
            builder.AppendLine($"{indent}  - slice {slice.Name} [{slice.Type}]: {SliceParts(slice)}");
        }

        foreach (var subFeature in feature.SubFeatures)
        {
            AppendFeature(builder, subFeature, featurePath, $"{indent}  ");
        }
    }

    static string SliceParts(ExtractedSlice slice) => string.Join("; ", Parts(slice));

    static IEnumerable<string> Parts(ExtractedSlice slice)
    {
        foreach (var command in slice.Commands)
        {
            yield return $"command {command.Name}({PropertyNames(command.Properties)})";
        }

        foreach (var @event in slice.Events)
        {
            yield return $"event {@event.Name}({PropertyNames(@event.Properties)})";
        }

        foreach (var readModel in slice.ReadModels)
        {
            yield return $"read model {readModel.Name}({PropertyNames(readModel.Properties)})";
        }

        foreach (var projection in slice.Projections)
        {
            yield return $"projection {projection.Name} from {string.Join(", ", projection.SourceEvents)}";
        }
    }

    static string PropertyNames(IReadOnlyList<ExtractedProperty> properties) =>
        string.Join(", ", properties.Select(property => property.Name));

    static string LoadTemplate()
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(ResourceName)
            ?? throw new MissingPromptResource(ResourceName);

        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }
}
