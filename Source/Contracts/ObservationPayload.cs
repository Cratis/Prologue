// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace Cratis.Prologue.Contracts;

/// <summary>
/// Represents the polymorphic, source-specific metadata carried by an <see cref="Observation"/>. New source kinds
/// extend the capture model by adding a new derived payload — nothing downstream needs to change. The type
/// discriminators are shared by the HTTP transport (System.Text.Json) and the Prologue API's MongoDB persistence.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(DatabaseTransactionObserved), DatabaseTransactionDiscriminator)]
[JsonDerivedType(typeof(DatabaseSchemaObserved), DatabaseSchemaDiscriminator)]
[JsonDerivedType(typeof(HttpCommandObserved), HttpCommandDiscriminator)]
[JsonDerivedType(typeof(TelemetryObserved), TelemetryDiscriminator)]
[JsonDerivedType(typeof(MetricObserved), MetricDiscriminator)]
[JsonDerivedType(typeof(LogObserved), LogDiscriminator)]
public abstract record ObservationPayload
{
    /// <summary>
    /// The discriminator identifying a <see cref="DatabaseTransactionObserved"/> payload.
    /// </summary>
    public const string DatabaseTransactionDiscriminator = "database-transaction";

    /// <summary>
    /// The discriminator identifying a <see cref="DatabaseSchemaObserved"/> payload.
    /// </summary>
    public const string DatabaseSchemaDiscriminator = "database-schema";

    /// <summary>
    /// The discriminator identifying a <see cref="HttpCommandObserved"/> payload.
    /// </summary>
    public const string HttpCommandDiscriminator = "http-command";

    /// <summary>
    /// The discriminator identifying a <see cref="TelemetryObserved"/> payload.
    /// </summary>
    public const string TelemetryDiscriminator = "telemetry";

    /// <summary>
    /// The discriminator identifying a <see cref="MetricObserved"/> payload.
    /// </summary>
    public const string MetricDiscriminator = "metric";

    /// <summary>
    /// The discriminator identifying a <see cref="LogObserved"/> payload.
    /// </summary>
    public const string LogDiscriminator = "log";
}
