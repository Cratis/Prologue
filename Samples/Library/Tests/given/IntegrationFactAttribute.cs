// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Library.Tests.given;

/// <summary>
/// A fact that needs the whole composition running. Nothing here can be hosted without a container runtime, so when
/// there is none the fact reports as skipped with the reason rather than failing over something the code under test
/// has no say in.
/// </summary>
/// <remarks>
/// The decision is made at discovery, which is the only place xUnit v2 lets a test opt out cleanly — there is no
/// runtime equivalent of <c>Assert.Skip</c> in that version.
/// </remarks>
public sealed class IntegrationFactAttribute : FactAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IntegrationFactAttribute"/> class.
    /// </summary>
    public IntegrationFactAttribute()
    {
        if (ContainerRuntime.UnavailableReason is { } reason)
        {
            Skip = reason;
        }
    }
}
