// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Scenarios;

namespace Cratis.Prologue.Workbench.Captures.Live;

/// <summary>
/// The captures labeled with a scenario.
/// </summary>
/// <param name="Scenario">The scenario.</param>
/// <param name="Captures">How many captures carry its label.</param>
public record ScenarioCaptures(ScenarioName Scenario, int Captures);
