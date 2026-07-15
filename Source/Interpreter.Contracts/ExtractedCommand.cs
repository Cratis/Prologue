// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Interpreter.Contracts;

/// <summary>
/// Represents a command inferred for a slice — the imperative intent behind an observed state-changing HTTP request
/// or telemetry span.
/// </summary>
/// <param name="Name">The imperative name of the command (for example <c>RegisterAuthor</c>).</param>
/// <param name="Properties">The inferred input properties of the command.</param>
public record ExtractedCommand(string Name, IReadOnlyList<ExtractedProperty> Properties);
