// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Explore.Matrix;

/// <summary>
/// In how many runs one kind of change happened to a table.
/// </summary>
/// <param name="Operation">What was done to a row.</param>
/// <param name="Runs">The runs (captures, or transactions for writes with no command) in which it happened at least once.</param>
public record OperationRuns(ChangeOperation Operation, int Runs);
