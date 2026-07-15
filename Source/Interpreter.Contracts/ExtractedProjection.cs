// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Interpreter.Contracts;

/// <summary>
/// Represents a projection inferred for a slice — the mapping that builds a read model by consuming the named
/// source events.
/// </summary>
/// <param name="Name">The name of the projection.</param>
/// <param name="SourceEvents">The names of the events the projection consumes to build its read model.</param>
public record ExtractedProjection(string Name, IReadOnlyList<string> SourceEvents);
