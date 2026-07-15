// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Contracts;
using Cratis.Prologue.Interpreter.Contracts;

namespace Cratis.Prologue.Interpreter;

/// <summary>
/// Represents an <see cref="IBuildHeuristicModel"/> that analyzes every capture into provisional slices and
/// aggregates the drafts that describe the same behavior into a single module → feature → slice tree.
/// </summary>
public class HeuristicModelBuilder : IBuildHeuristicModel
{
    /// <inheritdoc/>
    public ExtractionResult Build(Guid prologueId, IReadOnlyList<Capture> captures)
    {
        var drafts = captures.SelectMany(CaptureAnalyzer.Analyze).ToList();
        var modules = drafts
            .GroupBy(draft => draft.Module)
            .Select(moduleGroup => new ExtractedModule(moduleGroup.Key, BuildFeatures(moduleGroup)))
            .ToList();

        return new ExtractionResult(prologueId, modules);
    }

    static IReadOnlyList<ExtractedFeature> BuildFeatures(IEnumerable<SliceDraft> moduleDrafts) =>
    [
        .. moduleDrafts
            .GroupBy(draft => draft.Feature)
            .Select(featureGroup => new ExtractedFeature(featureGroup.Key, [], BuildSlices(featureGroup)))
    ];

    static IReadOnlyList<ExtractedSlice> BuildSlices(IEnumerable<SliceDraft> featureDrafts) =>
    [
        .. featureDrafts
            .GroupBy(draft => (draft.Name, draft.Type))
            .Select(sliceGroup => new ExtractedSlice(
                sliceGroup.Key.Name,
                sliceGroup.Key.Type,
                Merge(sliceGroup.SelectMany(draft => draft.Commands), command => command.Name),
                Merge(sliceGroup.SelectMany(draft => draft.Events), @event => @event.Name),
                Merge(sliceGroup.SelectMany(draft => draft.ReadModels), readModel => readModel.Name),
                MergeProjections(sliceGroup.SelectMany(draft => draft.Projections))))
    ];

    static IReadOnlyList<T> Merge<T>(IEnumerable<T> items, Func<T, string> keySelector) =>
    [
        .. items
            .GroupBy(keySelector)
            .Select(group => group.First())
    ];

    // Merge projections that build the same read model, unioning the events they consume — otherwise a read model
    // evidenced by several captures (for example authors created in one and deleted in another) would keep only the
    // first capture's events, so its downstream slice would reference the create event but not the delete.
    static IReadOnlyList<ExtractedProjection> MergeProjections(IEnumerable<ExtractedProjection> projections) =>
    [
        .. projections
            .GroupBy(projection => projection.Name)
            .Select(group => new ExtractedProjection(
                group.Key,
                [.. group.SelectMany(projection => projection.SourceEvents).Distinct(StringComparer.OrdinalIgnoreCase)]))
    ];
}
