// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Events.Suggestions;

/// <summary>
/// A command of the interpreted model.
/// </summary>
/// <param name="Name">The command name.</param>
/// <param name="Properties">The properties the command takes.</param>
/// <param name="Rules">The validation rules inferred for the properties.</param>
/// <param name="Description">What the command does, in words; empty when nobody described it.</param>
public record ModelCommand(CommandName Name, IEnumerable<SuggestedProperty> Properties, IEnumerable<ModelRule> Rules, string Description);
