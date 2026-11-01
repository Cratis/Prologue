// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Options;

namespace Cratis.Prologue.Workbench.Repositories;

/// <summary>
/// How cloning is configured on this Receiver. The directory is set in configuration, not in the Workbench.
/// </summary>
/// <param name="CloneDirectory">The directory repositories are cloned into.</param>
[ReadModel]
public record CloneSettings(string CloneDirectory)
{
    /// <summary>
    /// Gets the clone settings.
    /// </summary>
    /// <param name="options">The configured repository options.</param>
    /// <returns>The settings.</returns>
    public static CloneSettings GetCloneSettings(IOptions<RepositoryOptions> options) => new(options.Value.CloneDirectory);
}
