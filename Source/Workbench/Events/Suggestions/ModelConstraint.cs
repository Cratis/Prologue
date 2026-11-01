// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Events.Suggestions;

/// <summary>
/// A constraint the interpreter inferred from a unique index.
/// </summary>
/// <param name="Name">The constraint name.</param>
/// <param name="Property">The property that must be unique.</param>
/// <param name="Event">The suggested event the constraint holds on.</param>
public record ModelConstraint(string Name, string Property, SuggestionKey Event);
