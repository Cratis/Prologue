// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Explore.Matrix;

namespace Cratis.Prologue.Workbench.Explore.Uncorrelated;

/// <summary>
/// A table that database transactions with no command wrote.
/// </summary>
/// <param name="Write">What was done to the table. The runs are transactions.</param>
/// <param name="Transactions">How many such transactions touched it.</param>
/// <param name="Hours">When during the day they happened, hours with transactions only.</param>
public record UncorrelatedTable(TableWrite Write, int Transactions, IEnumerable<HourCount> Hours);
