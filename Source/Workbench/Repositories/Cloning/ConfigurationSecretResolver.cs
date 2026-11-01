// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.DependencyInjection;
using Cratis.Prologue.Configuration;
using Cratis.Prologue.Workbench.Common;
using Microsoft.Extensions.Configuration;

namespace Cratis.Prologue.Workbench.Repositories.Cloning;

/// <summary>
/// Resolves a secret from configuration under <c language="text">Prologue:Secrets:&lt;name&gt;</c>, and then from the
/// environment variable of the same name with everything but letters and digits replaced by underscores, in upper case.
/// </summary>
/// <param name="configuration">The configuration.</param>
[Singleton]
public class ConfigurationSecretResolver(IConfiguration configuration) : ISecretResolver
{
    /// <inheritdoc/>
    public string? Resolve(SecretName name)
    {
        var key = name.Value;
        return SecretPlaceholders.Resolve(configuration, key);
    }
}
