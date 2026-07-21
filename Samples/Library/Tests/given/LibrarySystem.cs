// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Library.Tests.given;

/// <summary>
/// Binds every spec to the one running composition. Sharing it also serializes the specs, which matters: they all
/// add to the same library, so they identify their own rows by unique names rather than by an empty starting state.
/// </summary>
[CollectionDefinition(Name)]
public class LibrarySystem : ICollectionFixture<DistributedLibrary>
{
    /// <summary>
    /// The name specs reference the collection by.
    /// </summary>
    public const string Name = "The library system";
}
