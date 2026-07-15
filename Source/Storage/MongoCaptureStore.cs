// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Cratis.Prologue.Storage;

/// <summary>
/// Represents an <see cref="ICaptureStore"/> that persists captures into a MongoDB collection. Owns its own MongoDB
/// connection derived from <see cref="PrologueStorageOptions"/> so it can be dropped into any host without colliding
/// with that host's other MongoDB registrations.
/// </summary>
/// <param name="options">The Prologue storage options carrying the MongoDB connection details.</param>
public class MongoCaptureStore(IOptions<PrologueStorageOptions> options) : ICaptureStore
{
    readonly IMongoCollection<Capture> _captures = new MongoClient(options.Value.Mongo.ConnectionString)
        .GetDatabase(options.Value.Mongo.Database)
        .GetCollection<Capture>(options.Value.Mongo.Collection);

    /// <inheritdoc/>
    public Task Store(Capture capture, CancellationToken cancellationToken = default) =>
        _captures.InsertOneAsync(capture, cancellationToken: cancellationToken);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Capture>> GetForPrologue(Guid prologueId, CancellationToken cancellationToken = default) =>
        await _captures
            .Find(capture => capture.PrologueId == prologueId)
            .SortBy(capture => capture.Occurred)
            .ToListAsync(cancellationToken);
}
