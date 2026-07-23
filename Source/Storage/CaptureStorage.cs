// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;

namespace Cratis.Prologue.Storage;

/// <summary>
/// Centralizes the MongoDB serialization setup and dependency-injection registration for Prologue capture storage,
/// so every host that persists captures agrees on how concepts and polymorphic payloads are represented.
/// </summary>
public static class CaptureStorage
{
    /// <summary>
    /// Registers the global BSON serializers, conventions, and polymorphic class maps capture storage relies on.
    /// Safe to call more than once — every registration is guarded.
    /// </summary>
    public static void RegisterSerializers()
    {
        BsonSerializer.TryRegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        BsonSerializer.TryRegisterSerializer(new SourceKindSerializer());

        // The capture contract carries no MongoDB attributes; register the polymorphic payload discriminators here.
        if (!BsonClassMap.IsClassMapRegistered(typeof(ObservationPayload)))
        {
            // Captures are persisted as camelCase — the same representation the contracts agree on over HTTP (see
            // CaptureSerialization). Register the conventions BEFORE any class map so AutoMap picks up the camelCase
            // element names; registering them afterwards would leave the already-mapped types on PascalCase and the
            // stored camelCase fields (prologueId, entries, payload, …) would silently fail to bind.
            var conventions = new ConventionPack
            {
                new CamelCaseElementNameConvention(),
                new IgnoreExtraElementsConvention(true)
            };
            ConventionRegistry.Register("Prologue", conventions, type => type.Namespace?.StartsWith("Cratis.Prologue.Contracts") == true);

            BsonClassMap.RegisterClassMap<ObservationPayload>(map => map.SetIsRootClass(true));
            BsonClassMap.RegisterClassMap<DatabaseTransactionObserved>(map =>
            {
                map.AutoMap();
                map.SetDiscriminator(ObservationPayload.DatabaseTransactionDiscriminator);
            });
            BsonClassMap.RegisterClassMap<DatabaseSchemaObserved>(map =>
            {
                map.AutoMap();
                map.SetDiscriminator(ObservationPayload.DatabaseSchemaDiscriminator);
            });
            BsonClassMap.RegisterClassMap<HttpCommandObserved>(map =>
            {
                map.AutoMap();
                map.SetDiscriminator(ObservationPayload.HttpCommandDiscriminator);
            });
            BsonClassMap.RegisterClassMap<TelemetryObserved>(map =>
            {
                map.AutoMap();
                map.SetDiscriminator(ObservationPayload.TelemetryDiscriminator);
            });
            BsonClassMap.RegisterClassMap<MetricObserved>(map =>
            {
                map.AutoMap();
                map.SetDiscriminator(ObservationPayload.MetricDiscriminator);
            });
            BsonClassMap.RegisterClassMap<LogObserved>(map =>
            {
                map.AutoMap();
                map.SetDiscriminator(ObservationPayload.LogDiscriminator);
            });
        }
    }

    /// <summary>
    /// Registers the BSON serialization and the <see cref="ICaptureStore"/> so captures can be persisted directly.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add registrations to.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> the <see cref="PrologueStorageOptions"/> are bound from.</param>
    /// <returns>The <paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddPrologueCaptureStorage(this IServiceCollection services, IConfiguration configuration)
    {
        RegisterSerializers();
        services.Configure<PrologueStorageOptions>(configuration.GetSection(PrologueStorageOptions.SectionName));
        services.AddSingleton<ICaptureStore, MongoCaptureStore>();
        return services;
    }
}
