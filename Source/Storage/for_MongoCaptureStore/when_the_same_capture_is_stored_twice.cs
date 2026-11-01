// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Servers;

namespace Cratis.Prologue.Storage.for_MongoCaptureStore;

public class when_the_same_capture_is_stored_twice : Specification
{
    IMongoCollection<Capture> _collection;
    Capture _capture;
    Exception _duplicate;
    bool _storedAgain;
    bool _storedFirst;
    Exception _otherFailure;

    static MongoWriteException WriteFailure(ServerErrorCategory category, int code)
    {
        var error = (WriteError)typeof(WriteError)
            .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .Single(constructor => constructor.GetParameters().Length == 4)
            .Invoke([category, code, "write failed", new BsonDocument()]);
        return new MongoWriteException(new ConnectionId(new ServerId(new ClusterId(1), new System.Net.DnsEndPoint("localhost", 27017))), error, null, null);
    }

    void Establish()
    {
        _capture = new Capture(Guid.NewGuid(), DateTimeOffset.UtcNow, []);
        _collection = Substitute.For<IMongoCollection<Capture>>();
    }

    async Task Because()
    {
        _collection
            .InsertOneAsync(Arg.Any<Capture>(), Arg.Any<InsertOneOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(WriteFailure(ServerErrorCategory.DuplicateKey, 11000)));
        _duplicate = await Cratis.Specifications.Catch.Exception(async () => _storedAgain = await new MongoCaptureStore(_collection).Store(_capture));

        _collection
            .InsertOneAsync(Arg.Any<Capture>(), Arg.Any<InsertOneOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(WriteFailure(ServerErrorCategory.Uncategorized, 2)));
        _collection
            .InsertOneAsync(Arg.Any<Capture>(), Arg.Any<InsertOneOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _storedFirst = await new MongoCaptureStore(_collection).Store(_capture);

        _collection
            .InsertOneAsync(Arg.Any<Capture>(), Arg.Any<InsertOneOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(WriteFailure(ServerErrorCategory.Uncategorized, 2)));
        _otherFailure = await Cratis.Specifications.Catch.Exception(() => new MongoCaptureStore(_collection).Store(_capture));
    }

    [Fact] void should_treat_the_duplicate_as_stored() => _duplicate.ShouldBeNull();
    [Fact] void should_say_it_was_stored_already() => _storedAgain.ShouldBeFalse();
    [Fact] void should_say_a_new_capture_was_stored() => _storedFirst.ShouldBeTrue();
    [Fact] void should_still_fail_on_any_other_write_error() => _otherFailure.ShouldBeOfExactType<MongoWriteException>();
}
#endif
