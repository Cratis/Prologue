// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Cratis.Prologue.Storage;

/// <summary>
/// Represents a BSON serializer that persists a <see cref="SourceKind"/> as a plain string rather than a nested document.
/// </summary>
public class SourceKindSerializer : SerializerBase<SourceKind>
{
    /// <inheritdoc/>
    public override SourceKind Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args) =>
        new(context.Reader.ReadString());

    /// <inheritdoc/>
    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, SourceKind value) =>
        context.Writer.WriteString(value.Value);
}
