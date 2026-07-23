// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Interpreter.Contracts;
using Cratis.Screenplay.Syntax;

namespace Cratis.Prologue.Screenplay;

/// <summary>
/// Defines the generator that turns an extracted event model into a Cratis Screenplay document — the bridge from
/// what the interpreter observed to the <c>.play</c> file a developer continues authoring from.
/// </summary>
public interface IScreenplayGenerator
{
    /// <summary>
    /// Builds the Screenplay syntax tree for an extracted event model.
    /// </summary>
    /// <param name="result">The <see cref="ExtractionResult"/> to build the tree from.</param>
    /// <returns>The <see cref="ApplicationSyntax"/> describing the extracted model.</returns>
    ApplicationSyntax Build(ExtractionResult result);

    /// <summary>
    /// Generates the <c>.play</c> source text for an extracted event model.
    /// </summary>
    /// <param name="result">The <see cref="ExtractionResult"/> to generate from.</param>
    /// <returns>The rendered <c>.play</c> source text.</returns>
    string Generate(ExtractionResult result);
}
