// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Events.Suggestions;

/// <summary>
/// A read model of the interpreted model.
/// </summary>
/// <param name="Name">The read model name.</param>
/// <param name="Properties">The properties it holds.</param>
public record ModelReadModel(string Name, IEnumerable<SuggestedProperty> Properties);
