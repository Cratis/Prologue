// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Library.Core.Simulation;

/// <summary>
/// The words the simulation draws on so the generated data reads like a library's rather than like noise.
/// </summary>
public static class SimulationVocabulary
{
    /// <summary>
    /// Gets the tags the simulation attaches to books.
    /// </summary>
    public static readonly string[] Tags =
    [
        "science-fiction", "literary", "classic", "dystopian", "historical", "magical-realism",
        "modernist", "postcolonial", "experimental", "mystery", "biography", "poetry",
        "reference", "young-adult", "translated", "award-winner"
    ];
}
