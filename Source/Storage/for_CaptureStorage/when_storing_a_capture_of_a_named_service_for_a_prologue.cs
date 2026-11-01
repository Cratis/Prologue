// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace Cratis.Prologue.Storage.for_CaptureStorage;

/// <summary>
/// The Workbench hosts the Receiver alongside Chronicle, but captures stay in their own collection in the shape the
/// Interpreter reads: a stored capture must keep the prologue it belongs to and the service it was correlated within.
/// </summary>
public class when_storing_a_capture_of_a_named_service_for_a_prologue : Specification
{
    static readonly Guid _prologueId = Guid.Parse("5b0f6f4e-6d5c-4f43-9a0e-3d1d2c4f9a10");

    Capture _capture;
    BsonDocument _document;
    Capture _restored;

    void Establish()
    {
        CaptureStorage.RegisterSerializers();
        _capture = new Capture(
            Guid.NewGuid(),
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            [new Observation(SourceKind.Http, new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), new HttpCommandObserved("POST", "/authors", 201)) { Service = "catalog-service" }],
            _prologueId)
        {
            Service = "catalog-service"
        };
    }

    void Because()
    {
        _document = _capture.ToBsonDocument();
        _restored = BsonSerializer.Deserialize<Capture>(_document);
    }

    [Fact] void should_store_the_prologue_id_as_a_camel_cased_field() => _document.Contains("prologueId").ShouldBeTrue();
    [Fact] void should_store_the_service_name_as_a_plain_string() => _document["service"].AsString.ShouldEqual("catalog-service");
    [Fact] void should_keep_the_prologue_id_when_read_back() => _restored.PrologueId.ShouldEqual(_prologueId);
    [Fact] void should_keep_the_service_name_when_read_back() => _restored.Service.Value.ShouldEqual("catalog-service");
    [Fact] void should_keep_the_service_name_of_its_observations() => _restored.Entries[0].Service.Value.ShouldEqual("catalog-service");
}
#endif
