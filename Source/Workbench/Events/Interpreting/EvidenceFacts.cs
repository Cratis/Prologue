// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Decisions;
using Cratis.Prologue.Workbench.Explore.Matrix;

namespace Cratis.Prologue.Workbench.Events.Interpreting;

/// <summary>
/// What the Workbench knows beyond the captures themselves, given to the interpretation as evidence.
/// </summary>
/// <param name="Schema">The database tables the captures observed, with their keys and unique indexes.</param>
/// <param name="Shapes">The command shapes with their statuses, writes and scenario labels, as Explore shows them.</param>
/// <param name="CodePoints">The decision points Discover found in the code, with file and line.</param>
/// <param name="Decisions">The decisions the captures show, as Capture lists them, newest first.</param>
public record EvidenceFacts(
    IReadOnlyList<SchemaTable> Schema,
    IReadOnlyList<ShapeProfile> Shapes,
    IReadOnlyList<DecisionPoint> CodePoints,
    IReadOnlyList<BusinessDecision> Decisions)
{
    /// <summary>
    /// Gets facts that hold nothing, for a prologue nothing is known about.
    /// </summary>
    public static readonly EvidenceFacts None = new([], [], [], []);
}
