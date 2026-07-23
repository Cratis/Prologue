// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Interpretation;

/// <summary>
/// Derives the provisional module, feature, resource, and action names from an observed HTTP route — the coarse
/// domain location the LLM refinement pass later turns into real domain language.
/// </summary>
public static class Location
{
    /// <summary>
    /// Splits an HTTP route into the module it belongs to, the feature within that module, the singular resource
    /// the request acted on, and an optional trailing action. A leading <c>api</c> segment, query strings, and
    /// id-looking segments (GUIDs, numbers, and <c>{template}</c> placeholders) are ignored so they never bleed
    /// into names. The feature is the first collection segment (so every operation on a collection groups together),
    /// the resource is the last collection segment singularized, and a trailing non-collection segment is treated as
    /// an action (for example <c>/api/inventory/{isbn}/lost</c> → resource <c>Inventory</c>, action <c>Lost</c>).
    /// </summary>
    /// <param name="path">The observed request route or path, optionally including a query string.</param>
    /// <returns>A tuple of the module, feature, singular resource, and action (empty when there is none).</returns>
    public static (string Module, string Feature, string Resource, string Action) FromPath(string path)
    {
        var withoutQuery = path.Split('?')[0];
        var segments = withoutQuery
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Where(segment => !segment.Equals("api", StringComparison.OrdinalIgnoreCase) && !IsIdentifier(segment))
            .ToList();

        if (segments.Count == 0)
        {
            return ("General", "General", "Resource", string.Empty);
        }

        var module = Naming.Pascalize(segments[0]);
        var collections = segments.Where(IsCollection).ToList();

        // A trailing non-collection segment after the first is an action on the resource (for example
        // /api/inventory/{isbn}/lost → action "Lost"), not a resource of its own.
        var action = segments.Count > 1 && !IsCollection(segments[^1])
            ? Naming.Pascalize(segments[^1])
            : string.Empty;

        var feature = collections.Count > 0 ? Naming.Pascalize(collections[0]) : module;
        var resource = collections.Count > 0
            ? Naming.Singularize(Naming.Pascalize(collections[^1]))
            : Naming.Singularize(module);

        return (module, feature, resource, action);
    }

    // A collection segment is a plural noun (ends in 's'), for example "authors" or "books" — the segments that
    // name resources, as opposed to id placeholders or trailing action verbs like "lost".
    static bool IsCollection(string segment) =>
        segment.Length > 1 && segment.EndsWith('s') && !IsIdentifier(segment);

    static bool IsIdentifier(string segment) =>
        (segment.StartsWith('{') && segment.EndsWith('}')) ||
        Guid.TryParse(segment, out _) ||
        long.TryParse(segment, out _);
}
