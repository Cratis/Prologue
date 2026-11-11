// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Interpretation;
using Cratis.Prologue.Workbench.Captures;

namespace Cratis.Prologue.Workbench.Events.Interpreting;

/// <summary>
/// Traces every event the heuristics suggest back to the captures that evidence it, with the same analysis the heuristics ran.
/// </summary>
public static class CaptureAttribution
{
    /// <summary>
    /// Attributes the events of the captured commands.
    /// </summary>
    /// <param name="partitions">The parts of the captures that were interpreted.</param>
    /// <returns>For each module and mechanical event name, what the captures show.</returns>
    public static IReadOnlyDictionary<SuggestionKey, Attribution> Attribute(IReadOnlyList<CapturePartition> partitions)
    {
        var attributions = new Dictionary<SuggestionKey, Attribution>();
        foreach (var (service, captures) in partitions.SelectMany(ServicesOf))
        {
            AttributeService(service, captures, attributions);
        }

        return attributions;
    }

    /// <summary>
    /// Gets the key of a suggestion.
    /// </summary>
    /// <param name="module">The module as the heuristics named it.</param>
    /// <param name="mechanical">The mechanical event name.</param>
    /// <returns>The key.</returns>
    public static SuggestionKey KeyOf(string module, string mechanical) => $"{module}/{mechanical}";

    static IEnumerable<(ServiceName Service, List<Capture> Captures)> ServicesOf(CapturePartition partition) =>
        partition.Captures.GroupBy(capture => capture.Service).Select(service => (service.Key, service.ToList()));

    static void AttributeService(ServiceName service, List<Capture> captures, Dictionary<SuggestionKey, Attribution> attributions)
    {
        var schema = SchemaIndex.From(captures);
        foreach (var capture in captures)
        {
            AttributeCapture(service, capture, schema, attributions);
        }
    }

    static void AttributeCapture(ServiceName service, Capture capture, SchemaIndex schema, Dictionary<SuggestionKey, Attribution> attributions)
    {
        var shape = ScopedCaptures.ShapeOf(capture);
        if (shape.IsNone)
        {
            return;
        }

        var transactions = capture.Entries.Select(entry => entry.Payload).OfType<DatabaseTransactionObserved>().ToList();
        var seen = new HashSet<SuggestionKey>();
        foreach (var draft in CaptureAnalyzer.Analyze(capture, schema).Where(draft => draft.Events.Count > 0))
        {
            var module = service.IsDefault ? draft.Module : Naming.Pascalize(service.Value);
            CountEvents(draft, module, shape, seen, attributions);
            foreach (var (name, change) in transactions.SelectMany(TableEvents.In))
            {
                AttributionOf(attributions, KeyOf(module, name)).Add(change);
            }
        }
    }

    static void CountEvents(SliceDraft draft, string module, CommandShape shape, HashSet<SuggestionKey> seen, Dictionary<SuggestionKey, Attribution> attributions)
    {
        foreach (var key in draft.Events.Select(@event => KeyOf(module, @event.Name)))
        {
            var attribution = AttributionOf(attributions, key);
            if (seen.Add(key))
            {
                attribution.Count(shape);
            }
        }
    }

    static Attribution AttributionOf(Dictionary<SuggestionKey, Attribution> attributions, SuggestionKey key)
    {
        if (!attributions.TryGetValue(key, out var attribution))
        {
            attributions[key] = attribution = new Attribution();
        }

        return attribution;
    }
}
