// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Captures;
using Cratis.Prologue.Workbench.Explore.Scope;

namespace Cratis.Prologue.Workbench.Events.Interpreting;

/// <summary>
/// Applies the interpretation scope chosen in Explore to a prologue's captures: excluded shapes are left out and split shapes are interpreted
/// once per response status.
/// </summary>
public static class ScopedCaptures
{
    /// <summary>
    /// Gets the command shape of a capture.
    /// </summary>
    /// <param name="capture">The capture.</param>
    /// <returns>The shape; none when the capture holds no HTTP command.</returns>
    public static CommandShape ShapeOf(Capture capture) =>
        capture.Entries.Select(entry => entry.Payload).OfType<HttpCommandObserved>().FirstOrDefault() is { } http
            ? CommandShape.From(http.Method, http.Path)
            : CommandShape.None;

    /// <summary>
    /// Gets the captures that are left once the excluded shapes are taken out. Captures without a command and the captured schema always stay.
    /// </summary>
    /// <param name="captures">All captures of the prologue.</param>
    /// <param name="scope">The scope, or null when nothing was ever excluded or split.</param>
    /// <returns>The captures to interpret.</returns>
    public static IReadOnlyList<Capture> Included(IReadOnlyList<Capture> captures, InterpretationScope? scope) =>
        [.. captures.Where(capture => ShapeOf(capture).IsNone || InterpretationScopes.Includes(scope, ShapeOf(capture)))];

    /// <summary>
    /// Divides the captures into the parts that are interpreted on their own: one for everything not split, then one per response status for each split shape.
    /// The captured schema goes into every part, because it is what types the properties.
    /// </summary>
    /// <param name="captures">All captures of the prologue.</param>
    /// <param name="scope">The scope, or null when nothing was ever excluded or split.</param>
    /// <returns>The parts, the one holding everything not split first.</returns>
    public static IReadOnlyList<CapturePartition> Partition(IReadOnlyList<Capture> captures, InterpretationScope? scope)
    {
        var included = Included(captures, scope);
        var schema = included.Where(IsSchema).ToList();
        var rest = new List<Capture>(schema);
        var split = new List<(CommandShape Shape, int Status, List<Capture> Captures)>();

        foreach (var capture in included.Where(capture => !IsSchema(capture)))
        {
            var shape = ShapeOf(capture);
            if (!shape.IsNone && InterpretationScopes.SplitsByStatus(scope, shape))
            {
                var status = capture.Entries.Select(entry => entry.Payload).OfType<HttpCommandObserved>().First().StatusCode;
                var part = split.Find(candidate => candidate.Shape == shape && candidate.Status == status);
                if (part.Captures is null)
                {
                    part = (shape, status, [.. schema]);
                    split.Add(part);
                }

                part.Captures.Add(capture);
            }
            else
            {
                rest.Add(capture);
            }
        }

        return
        [
            new CapturePartition(rest, string.Empty),
            .. split.OrderBy(part => part.Shape.Value, StringComparer.Ordinal).ThenBy(part => part.Status).Select(part => new CapturePartition(part.Captures, $"Status{part.Status}"))
        ];
    }

    static bool IsSchema(Capture capture) =>
        capture.Entries.Select(entry => entry.Payload).OfType<DatabaseSchemaObserved>().Any() &&
        !capture.Entries.Select(entry => entry.Payload).OfType<HttpCommandObserved>().Any();
}
