// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cratis.Prologue.Configuration;

/// <summary>
/// Reads and writes <c>cratis-prologue.json</c> files — the dedicated configuration file every Prologue tool
/// expects. Consumers (Studio, the Cratis CLI, custom tooling) use this to produce configuration in the exact
/// format the tools bind, instead of hand-rolling JSON.
/// </summary>
public static class PrologueConfigurationFile
{
    /// <summary>
    /// The well-known file name of the Prologue configuration file.
    /// </summary>
    public const string FileName = "cratis-prologue.json";

    /// <summary>
    /// The environment variable that overrides where the configuration file is loaded from.
    /// </summary>
    public const string PathEnvironmentVariable = "PROLOGUE_CONFIG";

    /// <summary>
    /// Gets the <see cref="JsonSerializerOptions"/> used for the configuration file — camelCase properties,
    /// enums as strings and indented output, matching what the configuration binder expects.
    /// </summary>
    public static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Serializes the given configuration to the <c>cratis-prologue.json</c> format.
    /// </summary>
    /// <param name="configuration">The configuration to serialize.</param>
    /// <returns>The JSON content for the configuration file.</returns>
    public static string Write(PrologueConfiguration configuration) =>
        JsonSerializer.Serialize(configuration, SerializerOptions);

    /// <summary>
    /// Writes the given configuration to a <c>cratis-prologue.json</c> file at the given path.
    /// </summary>
    /// <param name="configuration">The configuration to write.</param>
    /// <param name="path">The path of the file to write.</param>
    /// <returns>Awaitable task.</returns>
    public static Task WriteToFile(PrologueConfiguration configuration, string path) =>
        File.WriteAllTextAsync(path, Write(configuration));

    /// <summary>
    /// Deserializes a <c>cratis-prologue.json</c> content string into a <see cref="PrologueConfiguration"/>.
    /// </summary>
    /// <param name="json">The JSON content to deserialize.</param>
    /// <returns>The deserialized configuration, or a default configuration when the content is empty.</returns>
    public static PrologueConfiguration Read(string json) =>
        JsonSerializer.Deserialize<PrologueConfiguration>(json, SerializerOptions) ?? new PrologueConfiguration();

    /// <summary>
    /// Reads a <c>cratis-prologue.json</c> file from the given path.
    /// </summary>
    /// <param name="path">The path of the file to read.</param>
    /// <returns>The deserialized configuration.</returns>
    public static async Task<PrologueConfiguration> ReadFromFile(string path) =>
        Read(await File.ReadAllTextAsync(path));

    /// <summary>
    /// Resolves the effective configuration file path for a tool — the <c>PROLOGUE_CONFIG</c> environment
    /// variable when set, otherwise <c>cratis-prologue.json</c> in the given base directory.
    /// </summary>
    /// <param name="basePath">The base directory to resolve the default path against.</param>
    /// <returns>The resolved configuration file path.</returns>
    public static string ResolvePath(string basePath) =>
        Environment.GetEnvironmentVariable(PathEnvironmentVariable) is { Length: > 0 } configured
            ? configured
            : Path.Combine(basePath, FileName);
}
