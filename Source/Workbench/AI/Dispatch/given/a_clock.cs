// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Workbench.AI.Dispatch.given;

/// <summary>
/// A clock that only moves when a spec moves it.
/// </summary>
public class a_clock : TimeProvider
{
    DateTimeOffset _now = new(2026, 11, 2, 10, 0, 0, TimeSpan.Zero);

    public void Advance(TimeSpan by) => _now += by;

    public override DateTimeOffset GetUtcNow() => _now;
}
#endif
