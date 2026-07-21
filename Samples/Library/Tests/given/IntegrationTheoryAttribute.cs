// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Library.Tests.given;

/// <summary>
/// A theory that needs the whole composition running — the shape every behavior spec takes, since each one is
/// written once and run against both frontends. Skips for the same reason, and in the same way, as
/// <see cref="IntegrationFactAttribute"/>.
/// </summary>
public sealed class IntegrationTheoryAttribute : TheoryAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IntegrationTheoryAttribute"/> class.
    /// </summary>
    public IntegrationTheoryAttribute()
    {
        if (ContainerRuntime.UnavailableReason is { } reason)
        {
            Skip = reason;
        }
    }
}
