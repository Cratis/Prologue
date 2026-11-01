// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Common;

namespace Cratis.Prologue.Workbench.Repositories.Cloning;

/// <summary>
/// Resolves a secret by its name at the moment it is needed. A value is never stored, logged or returned to a client.
/// </summary>
public interface ISecretResolver
{
    /// <summary>
    /// Resolves a secret.
    /// </summary>
    /// <param name="name">The name of the secret.</param>
    /// <returns>The secret's value, or null when no secret is configured under that name.</returns>
    string? Resolve(SecretName name);
}
