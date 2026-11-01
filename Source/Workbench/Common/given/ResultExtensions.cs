// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Workbench.Common.given;

/// <summary>
/// Reads the outcome of a command's decision in specs.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Gets the refusal a decision ended in.
    /// </summary>
    /// <typeparam name="TEvent">The event type.</typeparam>
    /// <param name="result">The decision.</param>
    /// <returns>The refusal, or null when the decision succeeded.</returns>
    public static ValidationResult? Refusal<TEvent>(this Result<TEvent, ValidationResult> result) =>
        result.TryGetError(out var error) ? error : null;

    /// <summary>
    /// Gets the event a decision ended in.
    /// </summary>
    /// <typeparam name="TEvent">The event type.</typeparam>
    /// <param name="result">The decision.</param>
    /// <returns>The event, or null when the decision was refused.</returns>
    public static TEvent? Appended<TEvent>(this Result<TEvent, ValidationResult> result)
        where TEvent : class =>
        result.TryGetResult(out var appended) ? appended : null;
}
#endif
