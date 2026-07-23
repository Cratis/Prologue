// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Interpreter.Contracts;

/// <summary>
/// Represents a single property inferred for a command, event, or read model — a name paired with the best-guess
/// type derived from the observed database columns and telemetry attributes.
/// </summary>
/// <param name="Name">The name of the property.</param>
/// <param name="Type">The inferred type of the property (for example <see langword="string"/>, <see langword="int"/>, or <see cref="Guid"/>).</param>
/// <param name="IsRequired">Whether the property must have a value, derived from the observed column's nullability.</param>
/// <param name="MaxLength">The maximum length of the property's value, derived from the observed column's declared maximum; 0 when unbounded.</param>
public record ExtractedProperty(string Name, string Type, bool IsRequired = false, int MaxLength = 0);
