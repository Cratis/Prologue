// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Discover.Findings;

/// <summary>
/// The findings of a prologue nothing was found for.
/// </summary>
public static class NoFindings
{
    /// <summary>
    /// Gets the empty findings of a prologue.
    /// </summary>
    /// <param name="id">The prologue.</param>
    /// <returns>The findings, with nothing in them.</returns>
    public static CodeFindings For(PrologueId id) => new(id, [], [], [], []);
}
