// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.AI.LanguageModels;

namespace Cratis.Prologue.Workbench.AI;

/// <summary>
/// Describes one of the Prologue agents.
/// </summary>
/// <param name="Purpose">The purpose the agent's language model is configured for.</param>
/// <param name="Name">The display name of the agent.</param>
/// <param name="Role">What the agent does for the Workbench.</param>
public record PrologueAgent(LanguageModelPurpose Purpose, string Name, string Role);
