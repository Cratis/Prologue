// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Configuration;

/// <summary>
/// The exception that is thrown when a <c language="text">${secret:&lt;name&gt;}</c> placeholder names a secret that is not configured.
/// The message names the secret and where to set it, never a value.
/// </summary>
/// <param name="name">The name of the secret.</param>
public class SecretNotConfigured(string name) : Exception(
    $"The secret '{name}' is not configured, or its name is not valid (1 to 64 letters, digits, '_', '.', '/' or '-'). Set the configuration value '{SecretPlaceholders.SectionName}:{name}' or the environment variable '{SecretPlaceholders.EnvironmentName(name)}'.");
