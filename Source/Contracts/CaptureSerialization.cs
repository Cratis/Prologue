// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Cratis.Json;

namespace Cratis.Prologue.Contracts;

/// <summary>
/// Centralizes the JSON serialization used to transport <see cref="Capture"/>s between the Prologue Engine and the
/// Prologue API, so both sides agree on how concepts and polymorphic payloads are represented.
/// </summary>
public static class CaptureSerialization
{
    /// <summary>
    /// Gets the <see cref="JsonSerializerOptions"/> used to serialize and deserialize captures. Web defaults
    /// (camelCase, case-insensitive) plus the Cratis concept converters so <see cref="SourceKind"/> and other
    /// <c>ConceptAs</c> values round-trip as their underlying primitive.
    /// </summary>
    public static readonly JsonSerializerOptions Options = Create();

    /// <summary>
    /// Adds the Cratis concept converters to the given <see cref="JsonSerializerOptions"/>.
    /// </summary>
    /// <param name="options">The options to add the converters to.</param>
    public static void AddConverters(JsonSerializerOptions options)
    {
        options.Converters.Add(new ConceptAsJsonConverterFactory());
        options.Converters.Add(new EnumerableConceptAsJsonConverterFactory());
    }

    static JsonSerializerOptions Create()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        AddConverters(options);
        return options;
    }
}
