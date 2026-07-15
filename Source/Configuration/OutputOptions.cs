// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Configuration;

/// <summary>
/// Represents the kind of output the extractor writes captures to.
/// </summary>
public enum OutputKind
{
    /// <summary>
    /// Send captures to the Prologue Receiver.
    /// </summary>
    Api,

    /// <summary>
    /// Write captures to rolling JSON files.
    /// </summary>
    Json
}

/// <summary>
/// Represents the configuration for where the extractor writes captured data.
/// </summary>
public class OutputOptions
{
    /// <summary>
    /// Gets or sets the kind of output to use.
    /// </summary>
    public OutputKind Kind { get; set; } = OutputKind.Api;

    /// <summary>
    /// Gets or sets the Prologue Receiver configuration, used when <see cref="Kind"/> is <see cref="OutputKind.Api"/>.
    /// </summary>
    public ApiOptions Api { get; set; } = new();

    /// <summary>
    /// Gets or sets the rolling JSON file configuration, used when <see cref="Kind"/> is <see cref="OutputKind.Json"/>.
    /// </summary>
    public JsonFileOptions Json { get; set; } = new();
}

/// <summary>
/// Represents the configuration for rolling JSON file output.
/// </summary>
public class JsonFileOptions
{
    /// <summary>
    /// Gets or sets the directory captures are written to.
    /// </summary>
    public string Directory { get; set; } = "./captures";

    /// <summary>
    /// Gets or sets the maximum number of entries written to a single file before rolling to the next.
    /// </summary>
    public int MaxEntriesPerFile { get; set; } = 10_000;
}
