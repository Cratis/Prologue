// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Interpreter;

/// <summary>
/// Centralizes the names of the Orleans storage providers the interpreter service uses, so grains and
/// registrations never rely on magic strings.
/// </summary>
public static class WellKnownStorageProviders
{
    /// <summary>
    /// The name of the primary MongoDB-backed grain storage provider.
    /// </summary>
    public const string Default = "MongoDbGrainStorage";
}
