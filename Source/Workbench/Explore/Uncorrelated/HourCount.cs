// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Explore.Uncorrelated;

/// <summary>
/// How many transactions with no command touched a table during one hour of the day.
/// </summary>
/// <param name="Hour">The hour of the day, 0 to 23, in UTC.</param>
/// <param name="Transactions">The transactions.</param>
public record HourCount(int Hour, int Transactions);
