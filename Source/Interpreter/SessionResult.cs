// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Interpreter.Contracts;

namespace Cratis.Prologue.Interpreter;

/// <summary>
/// Represents the outcome of a completed interpreter session — the extracted event model together with the
/// Screenplay document generated from it. Only available once the session has completed.
/// </summary>
/// <param name="ExtractionResult">The extracted event model.</param>
/// <param name="Screenplay">The generated <c>.play</c> source text.</param>
public record SessionResult(ExtractionResult ExtractionResult, string Screenplay);
