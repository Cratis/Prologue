// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Interpreter.Contracts;
using Cratis.Prologue.Screenplay;
using Cratis.Prologue.Workbench.Events.Accepted;

namespace Cratis.Prologue.Workbench.Model.Export;

/// <summary>
/// Turns an <see cref="AcceptedModel"/> into Screenplay text. The Screenplay syntax has no comments, so a slice that still
/// holds open suggestions is marked as a draft in its description, which the printer writes out and the compiler accepts.
/// </summary>
public static class ModelScreenplays
{
    /// <summary>The first words of the description of a slice that has open suggestions.</summary>
    public const string DraftMarker = "Draft: not yet accepted.";

    /// <summary>
    /// Maps the model to the extraction result the Screenplay generator reads, open suggestions included and their slices marked as drafts.
    /// </summary>
    /// <param name="model">The <see cref="AcceptedModel"/>.</param>
    /// <returns>The <see cref="ExtractionResult"/>.</returns>
    public static ExtractionResult Extraction(AcceptedModel model)
    {
        var drafts = model.Modules
            .SelectMany(module => module.Features.SelectMany(feature => feature.Slices.Where(slice => slice.IsOpen).Select(slice => (Module: module.Name.Value, Feature: feature.Name.Value, Slice: slice))))
            .ToDictionary(item => (item.Module, item.Feature, item.Slice.Name.Value), item => item.Slice);
        var mapped = AcceptedModelMapper.ToExtractionResult(model);
        var declared = new HashSet<string>(StringComparer.Ordinal);
        return mapped with
        {
            Modules = [.. mapped.Modules.Select(module => module with { Features = [.. module.Features.Select(feature => feature with { Slices = [.. feature.Slices.Select(slice => DeclaredOnce(declared, Mark(drafts, module.Name, feature.Name, slice)))] })] })]
        };
    }

    /// <summary>
    /// Generates the <c language="csharp">.play</c> text of the model.
    /// </summary>
    /// <param name="model">The <see cref="AcceptedModel"/>.</param>
    /// <param name="generator">The <see cref="IScreenplayGenerator"/>.</param>
    /// <returns>The <see cref="ModelScreenplay"/>; an empty play when nothing is accepted or open yet.</returns>
    public static ModelScreenplay From(AcceptedModel model, IScreenplayGenerator generator)
    {
        var extraction = Extraction(model);
        var play = extraction.Modules.Count == 0 ? string.Empty : generator.Generate(extraction);
        return new(model.Id, play, model.Modules.SelectMany(module => module.Features).SelectMany(feature => feature.Slices).Count(slice => slice.IsOpen));
    }

    // A read model is declared by one slice only: Screenplay refuses a second declaration, and a second projection of it.
    // The first slice to hold a read model keeps it with its projection.
    static ExtractedSlice DeclaredOnce(HashSet<string> declared, ExtractedSlice slice)
    {
        var kept = slice.ReadModels.Select((readModel, index) => (ReadModel: readModel, Index: index)).Where(item => declared.Add(item.ReadModel.Name)).ToList();
        var indexes = kept.Select(item => item.Index).ToHashSet();
        return slice with
        {
            ReadModels = [.. kept.Select(item => item.ReadModel)],
            Projections = [.. slice.Projections.Where((_, index) => index >= slice.ReadModels.Count || indexes.Contains(index))]
        };
    }

    static ExtractedSlice Mark(Dictionary<(string Module, string Feature, string Slice), AcceptedSlice> drafts, string module, string feature, ExtractedSlice slice)
    {
        if (!drafts.TryGetValue((module, feature, slice.Name), out var open))
        {
            return slice;
        }

        var names = string.Join(", ", open.Events.Where(@event => @event.IsOpen).Select(@event => @event.Name.Value));
        var marker = $"{DraftMarker} Open suggestions: {names}.";
        return slice with { Description = slice.Description.Length == 0 ? marker : $"{marker}{Environment.NewLine}{slice.Description}" };
    }
}
