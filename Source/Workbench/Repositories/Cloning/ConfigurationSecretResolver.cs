// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.DependencyInjection;
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
    /// <summary>
    /// Gets the environment variable a secret name resolves to.
    /// </summary>
    /// <param name="name">The name of the secret.</param>
    /// <returns>The variable's name.</returns>
    public static string EnvironmentName(string name) => new string([.. name.Select(character => char.IsAsciiLetterOrDigit(character) ? character : '_')]).ToUpperInvariant();

    /// <inheritdoc/>
    public string? Resolve(SecretName name)
    {
        var key = name.Value.Trim();
        if (key.Length == 0)
        {
            return null;
        }

        var configured = configuration[$"{RepositoryOptions.SecretsSectionName}:{key}"];
        if (!string.IsNullOrEmpty(configured))
        {
            return configured;
        }

        var environment = Environment.GetEnvironmentVariable(EnvironmentName(key));
        return string.IsNullOrEmpty(environment) ? null : environment;
    }
}
