// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Interpreter.Contracts;

/// <summary>
/// Represents a uniqueness constraint inferred for a slice — evidence from a database unique constraint that the
/// captured system treats a property on one of the slice's events as an identity.
/// </summary>
/// <param name="Name">The name of the constraint (for example <c>UniqueEmail</c>).</param>
/// <param name="Property">The camelCase name of the property the uniqueness spans.</param>
/// <param name="OnEvent">The name of the event that carries the property.</param>
public record ExtractedConstraint(string Name, string Property, string OnEvent);
