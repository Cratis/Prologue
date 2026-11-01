// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace Cratis.Prologue.Configuration;

/// <summary>
/// Resolves secrets by name and expands <c language="text">${secret:&lt;name&gt;}</c> placeholders in configuration values.
/// A secret is looked up in configuration under <c language="text">Prologue:Secrets:&lt;name&gt;</c> first, and then in the
/// environment variable named by <see cref="EnvironmentName"/>.
/// </summary>
public static partial class SecretPlaceholders
{
    /// <summary>
    /// The configuration section secrets are looked up in, by name.
    /// </summary>
    public const string SectionName = "Prologue:Secrets";

    /// <summary>
    /// The prefix of the environment variables secrets are read from. A secret never resolves from an arbitrary
    /// environment variable, so a name chosen in the Workbench cannot read a connection string or API key meant for the host.
    /// </summary>
    public const string EnvironmentPrefix = "PROLOGUE_SECRET_";

    [GeneratedRegex(@"^[A-Za-z0-9_./-]{1,64}$", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex ValidName { get; }

    [GeneratedRegex(@"\$\{secret:(?<name>[^}]*)\}", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex Placeholder { get; }

    /// <summary>
    /// Gets the environment variable a secret name resolves to: <see cref="EnvironmentPrefix"/> followed by the name with everything but
    /// letters and digits replaced by underscores, in upper case.
    /// </summary>
    /// <param name="name">The name of the secret.</param>
    /// <returns>The variable's name.</returns>
    public static string EnvironmentName(string name) =>
        EnvironmentPrefix + new string([.. name.Select(character => char.IsAsciiLetterOrDigit(character) ? character : '_')]).ToUpperInvariant();

    /// <summary>
    /// Determines whether a value is acceptable as the name of a secret: 1 to 64 letters, digits, underscores, dots, slashes and hyphens.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns>True when it is.</returns>
    public static bool IsValidName(string name) => ValidName.IsMatch(name);

    /// <summary>
    /// Resolves a secret by name.
    /// </summary>
    /// <param name="configuration">The configuration holding <c language="text">Prologue:Secrets</c>.</param>
    /// <param name="name">The name of the secret.</param>
    /// <returns>The secret's value, or null when it is not configured or the name is not valid.</returns>
    public static string? Resolve(IConfiguration configuration, string name)
    {
        var key = name.Trim();
        if (!IsValidName(key))
        {
            return null;
        }

        var configured = configuration[$"{SectionName}:{key}"];
        if (!string.IsNullOrEmpty(configured))
        {
            return configured;
        }

        var environment = Environment.GetEnvironmentVariable(EnvironmentName(key));
        return string.IsNullOrEmpty(environment) ? null : environment;
    }

    /// <summary>
    /// Replaces every <c language="text">${secret:&lt;name&gt;}</c> in a value with the secret it names.
    /// </summary>
    /// <param name="configuration">The configuration holding <c language="text">Prologue:Secrets</c>.</param>
    /// <param name="value">The configuration value.</param>
    /// <returns>The value with its placeholders expanded; the value itself when it holds none.</returns>
    /// <exception cref="SecretNotConfigured">A placeholder names a secret that is not configured. The message names the secret, never a value.</exception>
    public static string Expand(IConfiguration configuration, string value) =>
        Placeholder.Replace(value, match =>
        {
            var name = match.Groups["name"].Value;
            return Resolve(configuration, name)
                ?? throw new SecretNotConfigured(name);
        });

    /// <summary>
    /// Determines whether a value holds a secret placeholder.
    /// </summary>
    /// <param name="value">The configuration value.</param>
    /// <returns>True when it does.</returns>
    public static bool HasPlaceholder(string? value) => value is not null && Placeholder.IsMatch(value);
}
