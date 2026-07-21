// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Library.Core.Pages;

/// <summary>
/// Represents a rejection or failure the library API reported, in the shape the pages render it — the
/// <c>title</c> and <c>detail</c> of an RFC 7807 problem document.
/// </summary>
/// <param name="Title">The short summary of what went wrong.</param>
/// <param name="Detail">The explanation of why it went wrong.</param>
public sealed record ApiProblem(string Title, string Detail);

/// <summary>
/// Represents the outcome of a library API call whose response body is not needed.
/// </summary>
/// <param name="Problem">The problem the API reported, or <see langword="null"/> when the call succeeded.</param>
public sealed record ApiResult(ApiProblem? Problem)
{
    /// <summary>
    /// Gets a value indicating whether the call succeeded.
    /// </summary>
    public bool IsSuccess => Problem is null;
}

/// <summary>
/// Represents the outcome of a library API call that returns a value.
/// </summary>
/// <typeparam name="TValue">The type of value the call returns.</typeparam>
/// <param name="Value">The value the API returned, or <see langword="null"/> when the call did not succeed.</param>
/// <param name="Problem">The problem the API reported, or <see langword="null"/> when the call succeeded.</param>
public sealed record ApiResult<TValue>(TValue? Value, ApiProblem? Problem)
{
    /// <summary>
    /// Gets a value indicating whether the call succeeded.
    /// </summary>
    public bool IsSuccess => Problem is null;
}
