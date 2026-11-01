// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Captures;

namespace Cratis.Prologue.Workbench.Events.Boundaries;

/// <summary>
/// A rule that must hold at once across things the events would otherwise keep apart, derived from the evidence.
/// </summary>
/// <param name="Key">The identity that survives the evidence being derived again.</param>
/// <param name="Shape">The command shape the evidence is about.</param>
/// <param name="Rule">The rule, in words.</param>
/// <param name="Evidence">What the captures and the schema show.</param>
/// <param name="Options">The ways to hold it; the first is the one the interpreter suggests.</param>
public record ConsistencyBoundary(BoundaryKey Key, CommandShape Shape, string Rule, string Evidence, IEnumerable<BoundaryOption> Options);
