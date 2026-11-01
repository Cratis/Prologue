// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Cratis.Prologue.Storage;

/// <summary>
/// Represents an <see cref="ICaptureStore"/> that persists captures into a MongoDB collection. Owns its own MongoDB
/// connection derived from <see cref="PrologueStorageOptions"/> so it can be dropped into any host without colliding
/// with that host's other MongoDB registrations. Storing a capture is idempotent: the Extractor retries a post whose answer
/// it did not see, and the capture it sends again has the same id.
/// </summary>
public class MongoCaptureStore : ICaptureStore
{
    readonly IMongoCollection<Capture> _captures;

    /// <summary>
    /// Initializes a new instance of the <see cref="MongoCaptureStore"/> class.
    /// </summary>
    /// <param name="options">The Prologue storage options carrying the MongoDB connection details.</param>
    public MongoCaptureStore(IOptions<PrologueStorageOptions> options)
        : this(new MongoClient(options.Value.Mongo.ConnectionString)
            .GetDatabase(options.Value.Mongo.Database)
            .GetCollection<Capture>(options.Value.Mongo.Collection))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MongoCaptureStore"/> class over a collection.
    /// </summary>
    /// <param name="captures">The collection captures are stored in.</param>
    internal MongoCaptureStore(IMongoCollection<Capture> captures) => _captures = captures;

    /// <inheritdoc/>
    public async Task<bool> Store(Capture capture, CancellationToken cancellationToken = default)
    {
        try
        {
            await _captures.InsertOneAsync(capture, cancellationToken: cancellationToken);
            return true;
        }
        catch (MongoWriteException exception) when (exception.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            // The same capture arrived again: it is already stored, which is what the caller wants.
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Capture>> GetForPrologue(Guid prologueId, CancellationToken cancellationToken = default) =>
        await _captures
            .Find(capture => capture.PrologueId == prologueId)
            .SortBy(capture => capture.Occurred)
            .ToListAsync(cancellationToken);
}
