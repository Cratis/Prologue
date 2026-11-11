// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Configuration;

namespace Cratis.Prologue.Extractor.Capturing;

/// <summary>
/// Represents the services the extractor captures from and how an observation is attributed to one of them. The
/// default service owns the top-level databases plus every reverse-proxy cluster and every telemetry service name
/// no named service claims, so a configuration without named services captures exactly as a single service.
/// </summary>
public sealed class CaptureServices
{
    readonly Dictionary<string, ServiceName> _clusters = [with(StringComparer.OrdinalIgnoreCase)];
    readonly Dictionary<string, ServiceName> _telemetryServiceNames = [with(StringComparer.OrdinalIgnoreCase)];
    readonly HashSet<string> _defaultTelemetryServiceNames;

    CaptureServices(PrologueOptions options)
    {
        _defaultTelemetryServiceNames = new(options.OpenTelemetry.ServiceNames, StringComparer.OrdinalIgnoreCase);
        var services = new List<CaptureService> { new(ServiceName.Default, [.. options.SqlServer], [.. options.Postgres]) };

        foreach (var (name, service) in options.Services.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
        {
            var serviceName = new ServiceName(name);
            services.Add(new(serviceName, [.. service.SqlServer], [.. service.Postgres]));
            Claim(_clusters, "reverse-proxy cluster", service.Clusters.Count > 0 ? service.Clusters : [name], serviceName);
            Claim(_telemetryServiceNames, "telemetry service name", service.TelemetryServiceNames.Count > 0 ? service.TelemetryServiceNames : [name], serviceName);
        }

        All = services;
    }

    /// <summary>
    /// Gets every service captured from — the default service first, then the named services.
    /// </summary>
    public IReadOnlyList<CaptureService> All { get; }

    /// <summary>
    /// Resolves the services to capture from out of the Prologue configuration.
    /// </summary>
    /// <param name="options">The Prologue configuration.</param>
    /// <returns>The resolved <see cref="CaptureServices"/>.</returns>
    /// <exception cref="AmbiguousCaptureService">Thrown when two named services claim the same cluster or telemetry service name.</exception>
    public static CaptureServices From(PrologueOptions options) => new(options);

    /// <summary>
    /// Resolves the service HTTP commands proxied to a reverse-proxy cluster belong to.
    /// </summary>
    /// <param name="clusterId">The id of the cluster the request was routed to.</param>
    /// <returns>The named service claiming the cluster, otherwise <see cref="ServiceName.Default"/>.</returns>
    public ServiceName ForCluster(string? clusterId) =>
        clusterId is not null && _clusters.TryGetValue(clusterId, out var service) ? service : ServiceName.Default;

    /// <summary>
    /// Resolves the service telemetry reported under an OpenTelemetry <c language="csharp">service.name</c> belongs to.
    /// </summary>
    /// <param name="telemetryServiceName">The <c language="csharp">service.name</c> the telemetry was reported under.</param>
    /// <param name="service">The service the telemetry belongs to, when it is captured.</param>
    /// <returns>True when the telemetry is captured; false when it is claimed by no named service and the default service's filter excludes it.</returns>
    public bool TryResolveTelemetry(string telemetryServiceName, out ServiceName service)
    {
        if (_telemetryServiceNames.TryGetValue(telemetryServiceName, out var named))
        {
            service = named;
            return true;
        }

        service = ServiceName.Default;
        return _defaultTelemetryServiceNames.Count == 0 || _defaultTelemetryServiceNames.Contains(telemetryServiceName);
    }

    static void Claim(Dictionary<string, ServiceName> claims, string kind, IEnumerable<string> values, ServiceName service)
    {
        foreach (var value in values)
        {
            if (claims.TryGetValue(value, out var existing) && existing != service)
            {
                throw new AmbiguousCaptureService(kind, value, existing, service);
            }

            claims[value] = service;
        }
    }
}
