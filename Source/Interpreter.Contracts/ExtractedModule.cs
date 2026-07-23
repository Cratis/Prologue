// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Interpreter.Contracts;

/// <summary>
/// Represents a module inferred for a Prologue — a top-level domain area grouping related features.
/// </summary>
/// <param name="Name">The name of the module.</param>
/// <param name="Features">The features within the module.</param>
/// <param name="Description">The description of the domain area the module covers; empty when not derived.</param>
public record ExtractedModule(string Name, IReadOnlyList<ExtractedFeature> Features, string Description = "");
