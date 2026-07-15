// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using System.Text;
using System.Text.Json;
using Cratis.Prologue.Contracts;

namespace Cratis.Prologue.Interpreter;

/// <summary>
/// Builds the prompt that asks the language model to refine the mechanical names in an extracted event model into
/// idiomatic domain language, giving it the observed behavior as evidence and constraining it to return a pure
/// name-to-name map so it can never alter the structure. The instruction text lives in the embedded
/// <c>Prompts/extraction.txt</c> resource; this type only fills in the evidence and provisional names.
/// </summary>
public static class ExtractionPrompt
{
    const string ResourceName = "Cratis.Prologue.Interpreter.Prompts.extraction.txt";

    static readonly string _template = LoadTemplate();

    /// <summary>
    /// Builds the refinement prompt from the provisional names and the captures they were derived from.
    /// </summary>
    /// <param name="names">The provisional names to refine.</param>
    /// <param name="captures">The captures the model was derived from, used as naming evidence.</param>
    /// <returns>The prompt text.</returns>
    public static string Build(IReadOnlyList<string> names, IReadOnlyList<Capture> captures) =>
        _template
            .Replace("{EVIDENCE}", Evidence(captures))
            .Replace("{NAMES}", JsonSerializer.Serialize(names));

    static string LoadTemplate()
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{ResourceName}' not found.");

        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }

    static string Evidence(IReadOnlyList<Capture> captures)
    {
        var payloads = captures.SelectMany(capture => capture.Entries).Select(entry => entry.Payload).ToList();
        var builder = new StringBuilder();

        foreach (var http in payloads.OfType<HttpCommandObserved>().Select(http => $"{http.Method} {http.Path}").Distinct().Take(50))
        {
            builder.AppendLine($"- HTTP: {http}");
        }

        foreach (var table in payloads.OfType<DatabaseTransactionObserved>().SelectMany(transaction => transaction.Tables)
            .Select(table => $"{table.Table} [{table.Operation}]: {string.Join(", ", table.Columns)}").Distinct().Take(50))
        {
            builder.AppendLine($"- DB table {table}");
        }

        foreach (var span in payloads.OfType<TelemetryObserved>().Select(span => $"{span.ServiceName}: {span.Name}").Distinct().Take(50))
        {
            builder.AppendLine($"- Span: {span}");
        }

        return builder.ToString();
    }
}
