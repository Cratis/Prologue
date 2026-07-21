// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Library.Tests.given;

/// <summary>
/// The two frontends the library sample ships over the same API. They render the same markup skeleton and the same
/// <c>data-testid</c>s on purpose, so one suite drives both — the name is what the <c>frontend-kind</c> badge reads.
/// </summary>
public enum Frontend
{
    /// <summary>
    /// The server-rendered frontend Core serves, reached through the extractor's proxy so its traffic is captured.
    /// </summary>
    Razor,

    /// <summary>
    /// The client-rendered single-page frontend Vite serves, calling the API through the extractor's proxy.
    /// </summary>
    React
}
