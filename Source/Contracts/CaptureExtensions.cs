// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Contracts;

/// <summary>
/// Provides extension methods for working with sets of <see cref="Capture"/>s.
/// </summary>
public static class CaptureExtensions
{
    /// <summary>
    /// Narrows captures to those of one service.
    /// </summary>
    /// <param name="captures">The captures to narrow.</param>
    /// <param name="service">The service to keep; <see langword="null"/> keeps the captures of every service.</param>
    /// <returns>The captures of the given service, or every capture when no service is given.</returns>
    public static IReadOnlyList<Capture> OfService(this IEnumerable<Capture> captures, ServiceName? service) =>
        service is null ? [.. captures] : [.. captures.Where(capture => capture.Service == service)];

    /// <summary>
    /// Lists the distinct services the captures belong to, the default service first and the named ones in order.
    /// </summary>
    /// <param name="captures">The captures to list the services of.</param>
    /// <returns>The distinct services.</returns>
    public static IReadOnlyList<ServiceName> Services(this IEnumerable<Capture> captures) =>
        [.. captures
            .Select(capture => capture.Service)
            .Distinct()
            .OrderBy(service => service.IsDefault ? 0 : 1)
            .ThenBy(service => service.Value, StringComparer.OrdinalIgnoreCase)];
}
