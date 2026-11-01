// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Captures.Live;

/// <summary>
/// The captures that arrived in one minute.
/// </summary>
/// <param name="Minute">The start of the minute, in UTC.</param>
/// <param name="Captures">How many captures arrived.</param>
public record MinuteBucket(DateTimeOffset Minute, int Captures);
