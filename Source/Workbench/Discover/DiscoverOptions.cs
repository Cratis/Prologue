// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Discover;

/// <summary>
/// Configures what Discover may read on the machine the Receiver runs on.
/// </summary>
public class DiscoverOptions
{
    /// <summary>
    /// The configuration section the options are bound from.
    /// </summary>
    public const string SectionName = "Prologue:Discover";

    /// <summary>
    /// Gets or sets the folders analysis is limited to. Empty, the default, means any folder the Receiver can read may be analyzed;
    /// when folders are listed, a folder to analyze has to be one of them or inside one.
    /// </summary>
    public IList<string> AllowedRoots { get; set; } = [];
}
