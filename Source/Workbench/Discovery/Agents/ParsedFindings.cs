// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Discovery.Agents;

/// <summary>
/// What could be read from an agent's answer.
/// </summary>
/// <typeparam name="T">The kind of finding.</typeparam>
/// <param name="Items">The valid findings.</param>
/// <param name="Dropped">Why each entry that was dropped was not usable, so the loss is logged rather than silent.</param>
public record ParsedFindings<T>(IReadOnlyList<T> Items, IReadOnlyList<string> Dropped);
