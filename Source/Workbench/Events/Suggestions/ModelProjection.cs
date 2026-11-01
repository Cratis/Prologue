// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Events.Suggestions;

/// <summary>
/// A projection of the interpreted model: how events build a read model.
/// </summary>
/// <param name="Name">The projection name.</param>
/// <param name="Events">The suggested events it consumes.</param>
public record ModelProjection(string Name, IEnumerable<SuggestionKey> Events);
