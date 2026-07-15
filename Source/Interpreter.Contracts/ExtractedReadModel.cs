// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Interpreter.Contracts;

/// <summary>
/// Represents a read model inferred for a slice — the queryable state a projection builds from the slice's events.
/// </summary>
/// <param name="Name">The name of the read model.</param>
/// <param name="Properties">The inferred properties of the read model.</param>
public record ExtractedReadModel(string Name, IReadOnlyList<ExtractedProperty> Properties);
