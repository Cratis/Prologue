// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Nodes;

namespace Cratis.Prologue.Configuration;

/// <summary>
/// Represents the full content of a <c>cratis-prologue.json</c> configuration file — the single configuration
/// file every Prologue tool reads. The Extractor binds <see cref="Prologue"/>; the Interpreter binds <see cref="Llm"/>.
/// </summary>
public class PrologueConfiguration
{
    /// <summary>
    /// Gets or sets the capture configuration used by the Extractor.
    /// </summary>
    public PrologueOptions Prologue { get; set; } = new();

    /// <summary>
    /// Gets or sets the language-model refinement configuration used by the Interpreter.
    /// </summary>
    public LlmOptions Llm { get; set; } = new();

    /// <summary>
    /// Gets or sets the YARP reverse-proxy configuration the Extractor's HTTP command capture uses — the routes and
    /// clusters pointing at the system being captured. Kept as raw JSON since it follows YARP's own schema.
    /// </summary>
    public JsonObject? ReverseProxy { get; set; }
}
