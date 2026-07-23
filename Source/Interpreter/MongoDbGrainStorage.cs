// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MongoDB.Driver;
using Orleans.Storage;

namespace Cratis.Prologue.Interpreter;

/// <summary>
/// Represents an <see cref="IGrainStorage"/> that persists grain state documents in a MongoDB collection, keyed by
/// the grain's <see cref="Guid"/> identity.
/// </summary>
/// <typeparam name="TState">The type of state document stored in the collection.</typeparam>
/// <param name="collection">The <see cref="IMongoCollection{TState}"/> the state documents are stored in.</param>
public class MongoDbGrainStorage<TState>(IMongoCollection<TState> collection) : IGrainStorage
{
    const string IdentityField = "_id";

    /// <inheritdoc/>
    public async Task ReadStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        var document = await collection.Find(FilterFor(grainId)).FirstOrDefaultAsync();
        if (document is not null)
        {
            grainState.State = (T)(object)document;
            grainState.RecordExists = true;
        }
    }

    /// <inheritdoc/>
    public async Task WriteStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        await collection.ReplaceOneAsync(FilterFor(grainId), (TState)(object)grainState.State!, new ReplaceOptions { IsUpsert = true });
        grainState.RecordExists = true;
    }

    /// <inheritdoc/>
    public async Task ClearStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        await collection.DeleteOneAsync(FilterFor(grainId));
        grainState.RecordExists = false;
    }

    static FilterDefinition<TState> FilterFor(GrainId grainId) =>
        Builders<TState>.Filter.Eq(IdentityField, grainId.GetGuidKey());
}
