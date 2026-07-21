// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net;

namespace Library.Tests.Extraction.given;

/// <summary>
/// The exception that is thrown when the simulation status could not be read from the API.
/// </summary>
/// <param name="statusCode">The status the API answered with.</param>
public class SimulationUnreadable(HttpStatusCode statusCode)
    : Exception($"The simulation status could not be read; the API answered {(int)statusCode} {statusCode}.");
