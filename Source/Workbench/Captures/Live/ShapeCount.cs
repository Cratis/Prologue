// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Captures;
using Cratis.Prologue.Workbench.Scenarios;

namespace Cratis.Prologue.Workbench.Captures.Live;

/// <summary>
/// How often a command shape was captured, and how it was answered.
/// </summary>
/// <param name="Shape">The command shape.</param>
/// <param name="Captures">How many captures hold it.</param>
/// <param name="Accepted">How many of them the system accepted.</param>
/// <param name="Refused">How many of them the system refused.</param>
/// <param name="LastScenario">The scenario the latest capture of the shape was labeled with.</param>
public record ShapeCount(CommandShape Shape, int Captures, int Accepted, int Refused, ScenarioName LastScenario);
