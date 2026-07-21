// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;

namespace Cratis.Prologue.Configuration;

/// <summary>
/// Adds the Prologue configuration file to a configuration builder, with the precedence every Prologue tool
/// expects.
/// </summary>
public static class PrologueConfigurationExtensions
{
    /// <summary>
    /// Adds <c>cratis-prologue.json</c> as the base configuration and then re-applies environment variables on top
    /// of it.
    /// </summary>
    /// <remarks>
    /// The order matters, and getting it wrong is silent. A host builder has already added environment variables
    /// by the time a tool adds its configuration file, and the last source added wins — so adding the file on its
    /// own makes the file override the environment. That is backwards: the file is the checked-in baseline, while
    /// the environment is how a container, an orchestrator, or an Aspire composition configures a deployed tool.
    /// Re-adding the environment variables afterwards restores the expected precedence.
    /// </remarks>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add the configuration to.</param>
    /// <param name="basePath">The base directory the configuration file is resolved against when <c>PROLOGUE_CONFIG</c> is not set.</param>
    /// <param name="reloadOnChange">Whether to reload the configuration when the file changes.</param>
    /// <returns>The <paramref name="builder"/> for chaining.</returns>
    public static IConfigurationBuilder AddPrologueConfiguration(
        this IConfigurationBuilder builder,
        string basePath,
        bool reloadOnChange = false)
    {
        builder.AddJsonFile(PrologueConfigurationFile.ResolvePath(basePath), optional: true, reloadOnChange);
        builder.AddEnvironmentVariables();

        return builder;
    }
}
