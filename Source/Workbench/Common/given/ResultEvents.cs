// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Workbench.Common.given;

/// <summary>
/// Reads the events a decision that may append several or none ended in.
/// </summary>
public static class ResultEvents
{
    /// <summary>
    /// Gets the refusal a decision ended in.
    /// </summary>
    /// <typeparam name="TEvent">The event type.</typeparam>
    /// <param name="result">The decision.</param>
    /// <returns>The refusal, or null when the decision succeeded.</returns>
    public static ValidationResult? Refusal<TEvent>(this Result<IEnumerable<TEvent>, ValidationResult> result) =>
        result.TryGetError(out var error) ? error : null;

    /// <summary>
    /// Gets the events a decision ended in.
    /// </summary>
    /// <typeparam name="TEvent">The event type.</typeparam>
    /// <param name="result">The decision.</param>
    /// <returns>The events, none when the decision was refused or recorded nothing.</returns>
    public static IReadOnlyList<TEvent> Events<TEvent>(this Result<IEnumerable<TEvent>, ValidationResult> result) =>
        result.TryGetResult(out var appended) ? [.. appended] : [];
}
#endif
