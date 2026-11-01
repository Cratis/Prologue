// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Captures;
using Cratis.Prologue.Workbench.Captures.Live;
using Cratis.Prologue.Workbench.Explore.Observations;

namespace Cratis.Prologue.Workbench.Explore.Matrix;

/// <summary>
/// Everything captured about one command shape: how often, how it was answered, what it wrote and what is worth noticing.
/// </summary>
/// <param name="Shape">The command shape.</param>
/// <param name="Captures">How many captures hold it.</param>
/// <param name="WithoutWrites">How many of them changed no table.</param>
/// <param name="Statuses">How it was answered, most common first, each with what it wrote.</param>
/// <param name="Writes">What it wrote across all statuses, per table.</param>
/// <param name="Handlers">The spans that began its captures, most common first.</param>
/// <param name="Scenarios">The captures per scenario label.</param>
/// <param name="Observations">What is worth noticing in the numbers above.</param>
public record ShapeProfile(
    CommandShape Shape,
    int Captures,
    int WithoutWrites,
    IEnumerable<StatusProfile> Statuses,
    IEnumerable<TableWrite> Writes,
    IEnumerable<HandlerCount> Handlers,
    IEnumerable<ScenarioCaptures> Scenarios,
    IEnumerable<ShapeObservation> Observations);
