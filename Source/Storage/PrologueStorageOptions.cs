// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Storage;

/// <summary>
/// Represents the configuration for Prologue capture storage, bound from the <c>Prologue</c> configuration section.
/// </summary>
public class PrologueStorageOptions
{
    /// <summary>
    /// The configuration section name the options are bound from.
    /// </summary>
    public const string SectionName = "Prologue";

    /// <summary>
    /// Gets or sets the MongoDB configuration captures are persisted to.
    /// </summary>
    public MongoOptions Mongo { get; set; } = new();
}
