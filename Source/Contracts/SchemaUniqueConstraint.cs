// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Contracts;

/// <summary>
/// Represents a unique constraint or unique index on an observed table — evidence that the captured system
/// treats the combination of these columns as an identity.
/// </summary>
/// <param name="Name">The name of the constraint or index.</param>
/// <param name="Columns">The names of the columns the uniqueness spans, in constraint order.</param>
public record SchemaUniqueConstraint(string Name, IReadOnlyList<string> Columns);
