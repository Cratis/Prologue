// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Library.Core.Database;

/// <summary>
/// The exception that is thrown when the library system starts without a connection string for its database.
/// </summary>
/// <param name="name">The name of the missing connection string.</param>
public class LibraryDatabaseNotConfigured(string name)
    : Exception($"No connection string named '{name}' is configured. The Aspire composition supplies it; when running the Core project on its own, set ConnectionStrings__{name}.");
