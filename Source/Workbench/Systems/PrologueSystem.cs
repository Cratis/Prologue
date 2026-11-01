// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Systems;

/// <summary>
/// A system in a prologue: one named service, where its code lives and what is captured from it at runtime.
/// </summary>
/// <param name="Id">The identity of the system.</param>
/// <param name="Name">The system's name, which is its service name in the configuration.</param>
/// <param name="Location">Where the code lives, once it has been located.</param>
/// <param name="HttpClusters">The HTTP clusters the system is reached through.</param>
/// <param name="Databases">The databases read for the system.</param>
/// <param name="TelemetryServices">The OpenTelemetry service names the system reports as.</param>
/// <param name="AttributeKeys">The span attribute keys whose values may be captured.</param>
public record PrologueSystem(
    SystemId Id,
    ServiceName Name,
    SystemCodeLocation? Location,
    IEnumerable<ClusterName> HttpClusters,
    IEnumerable<DatabaseSource> Databases,
    IEnumerable<TelemetryServiceName> TelemetryServices,
    IEnumerable<AttributeKey> AttributeKeys);
