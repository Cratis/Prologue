// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Repositories;

/// <summary>
/// Configuration for the repositories the Workbench clones, bound from <c language="text">Prologue:Repositories</c>.
/// </summary>
public class RepositoryOptions
{
    /// <summary>
    /// The configuration section the options are bound from.
    /// </summary>
    public const string SectionName = "Prologue:Repositories";

    /// <summary>
    /// The configuration section secrets are looked up in, by name, when a clone needs a credential.
    /// </summary>
    public const string SecretsSectionName = Cratis.Prologue.Configuration.SecretPlaceholders.SectionName;

    /// <summary>
    /// Gets or sets the directory repositories are cloned into.
    /// </summary>
    public string CloneDirectory { get; set; } = "/var/prologue/repos";
}
