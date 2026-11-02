// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Repositories.Cloning;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cratis.Prologue.Workbench.Repositories;

/// <summary>
/// Registers cloning of the repositories systems live in.
/// </summary>
public static class RepositoryServiceCollectionExtensions
{
    /// <summary>
    /// Adds the clone directory configuration, the git runner, the secret resolver, the installation tokens and the cloner.
    /// </summary>
    /// <param name="builder">The <see cref="WebApplicationBuilder"/> to add to.</param>
    /// <returns>The builder for continuation.</returns>
    public static WebApplicationBuilder AddRepositories(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<RepositoryOptions>(builder.Configuration.GetSection(RepositoryOptions.SectionName));
        builder.Services.AddSingleton<IGitRunner, GitRunner>();
        builder.Services.AddSingleton<ISecretResolver, ConfigurationSecretResolver>();
        builder.Services.AddHttpClient();
        builder.Services.TryAddSingleton(TimeProvider.System);
        builder.Services.AddSingleton<IInstallationTokens, InstallationTokens>();
        builder.Services.AddSingleton<IRepositoryCloner, RepositoryCloner>();
        return builder;
    }
}
