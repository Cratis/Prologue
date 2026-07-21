// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Library.Tests.given;

/// <summary>
/// The exception that is thrown when a row a spec needs to act on never turned up in the table it was expected in.
/// </summary>
/// <param name="table">The <c>data-testid</c> of the table that was looked in.</param>
/// <param name="text">The text the row was recognized by.</param>
public class RowNotFound(string table, string text)
    : Exception($"No row containing '{text}' turned up in '{table}'.");
