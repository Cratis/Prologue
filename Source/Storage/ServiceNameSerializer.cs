// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Cratis.Prologue.Storage;

/// <summary>
/// Represents a BSON serializer that persists a <see cref="ServiceName"/> as a plain string rather than a nested document.
/// </summary>
public class ServiceNameSerializer : SerializerBase<ServiceName>
{
    /// <inheritdoc/>
    public override ServiceName Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args) =>
        new(context.Reader.ReadString());

    /// <inheritdoc/>
    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, ServiceName value) =>
        context.Writer.WriteString(value.Value);
}
