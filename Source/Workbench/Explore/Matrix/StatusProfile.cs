// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Captures;

namespace Cratis.Prologue.Workbench.Explore.Matrix;

/// <summary>
/// The captures of a command shape that were answered with one status, and what they wrote.
/// </summary>
/// <param name="Status">The status.</param>
/// <param name="Captures">How many captures were answered with it.</param>
/// <param name="WithoutWrites">How many of them changed no table.</param>
/// <param name="Writes">What they wrote, per table.</param>
public record StatusProfile(HttpStatus Status, int Captures, int WithoutWrites, IEnumerable<TableWrite> Writes);
