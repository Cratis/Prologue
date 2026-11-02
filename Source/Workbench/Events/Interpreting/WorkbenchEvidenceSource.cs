// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.ReadModels;
using Cratis.Prologue.Storage;
using Cratis.Prologue.Workbench.Captures.Live;
using Cratis.Prologue.Workbench.Decisions;
using Cratis.Prologue.Workbench.Discover.Findings;
using Cratis.Prologue.Workbench.Events.Suggestions;
using Cratis.Prologue.Workbench.Events.Uncaptured;
using Cratis.Prologue.Workbench.Explore.Matrix;
using Cratis.Prologue.Workbench.Explore.Scope;
using Cratis.Prologue.Workbench.Explore.Uncorrelated;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Events.Interpreting;

/// <summary>
/// Represents an <see cref="IEvidenceSource"/> that reads the captures from the capture store the Interpreter reads too, and the rest from the Workbench's read models.
/// </summary>
/// <param name="captures">The capture store.</param>
/// <param name="readModels">Reads the Workbench's read models.</param>
public class WorkbenchEvidenceSource(ICaptureStore captures, IReadModels readModels) : IEvidenceSource
{
    /// <inheritdoc/>
    public async Task<InterpretationInput> Load(PrologueId prologue, CancellationToken cancellationToken)
    {
        var stored = await captures.GetForPrologue(prologue.Value, cancellationToken);
        var scope = await readModels.GetInstanceById<InterpretationScope>((EventSourceId)prologue);
        var matrix = await readModels.GetInstanceById<CommandMatrix>((EventSourceId)prologue);
        var findings = await readModels.GetInstanceById<CodeFindings>((EventSourceId)prologue);
        var recent = await readModels.GetInstanceById<RecentCaptures>((EventSourceId)prologue);
        var review = await readModels.GetInstanceById<EventReview>((EventSourceId)prologue);
        var uncorrelated = await readModels.GetInstanceById<UncorrelatedWrites>((EventSourceId)prologue);

        var points = findings is null ? [] : FindingsDecisionPoints.From(findings);
        var facts = new EvidenceFacts(
            Schema(stored),
            [.. matrix?.Shapes ?? []],
            points,
            recent is null ? [] : [.. BusinessDecision.From(recent, points)])
        {
            Capabilities = [.. findings?.Capabilities ?? []],
            Jobs = findings is null ? [] : CodeJobs.From(findings),
            Uncorrelated = [.. uncorrelated?.Tables ?? []]
        };

        return new InterpretationInput(stored, scope?.Excluded is null ? null : scope, facts, [.. review?.Answers ?? []]);
    }

    static IReadOnlyList<SchemaTable> Schema(IEnumerable<Capture> captures) =>
    [
        .. captures
            .SelectMany(capture => capture.Entries)
            .Select(entry => entry.Payload)
            .OfType<DatabaseSchemaObserved>()
            .SelectMany(schema => schema.Tables)
            .GroupBy(table => (table.Schema, table.Table))
            .Select(group => group.Last())
    ];
}
