// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cratis.Prologue.Interpreter.Contracts;

/// <summary>
/// Reads and writes the extraction result file the Interpreter produces — the file consumers (Studio, the CLI)
/// pick up from the Interpreter's mounted output folder. Both sides use this type so the format always agrees.
/// </summary>
public static class ExtractionResultFile
{
    /// <summary>
    /// The well-known file name of the extraction result file.
    /// </summary>
    public const string FileName = "extraction-result.json";

    /// <summary>
    /// Gets the <see cref="JsonSerializerOptions"/> used for the extraction result file — camelCase properties,
    /// enums as strings and indented output.
    /// </summary>
    public static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Serializes an <see cref="ExtractionResult"/> to its file representation.
    /// </summary>
    /// <param name="result">The result to serialize.</param>
    /// <returns>The JSON content for the result file.</returns>
    public static string Write(ExtractionResult result) => JsonSerializer.Serialize(result, SerializerOptions);

    /// <summary>
    /// Writes an <see cref="ExtractionResult"/> to a file at the given path, creating the folder when needed.
    /// </summary>
    /// <param name="result">The result to write.</param>
    /// <param name="path">The path of the file to write.</param>
    /// <returns>Awaitable task.</returns>
    public static Task WriteToFile(ExtractionResult result, string path)
    {
        if (Path.GetDirectoryName(path) is { Length: > 0 } folder)
        {
            Directory.CreateDirectory(folder);
        }

        return File.WriteAllTextAsync(path, Write(result));
    }

    /// <summary>
    /// Deserializes an extraction result from its file representation.
    /// </summary>
    /// <param name="json">The JSON content to deserialize.</param>
    /// <returns>The deserialized <see cref="ExtractionResult"/>, or <see langword="null"/> when the content is empty.</returns>
    public static ExtractionResult? Read(string json) => JsonSerializer.Deserialize<ExtractionResult>(json, SerializerOptions);

    /// <summary>
    /// Reads an extraction result file from the given path.
    /// </summary>
    /// <param name="path">The path of the file to read.</param>
    /// <returns>The deserialized <see cref="ExtractionResult"/>, or <see langword="null"/> when the file holds no result.</returns>
    public static async Task<ExtractionResult?> ReadFromFile(string path) => Read(await File.ReadAllTextAsync(path));
}
