// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Storage;

/// <summary>
/// Represents the MongoDB configuration used to store captures.
/// </summary>
public class MongoOptions
{
    /// <summary>
    /// Gets or sets the MongoDB connection string.
    /// </summary>
    public string ConnectionString { get; set; } = "mongodb://localhost:27017";

    /// <summary>
    /// Gets or sets the database name captures are stored in.
    /// </summary>
    public string Database { get; set; } = "Prologue";

    /// <summary>
    /// Gets or sets the collection name captures are stored in.
    /// </summary>
    public string Collection { get; set; } = "captures";
}
