// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Discover.Agents;

/// <summary>
/// A batch of source files an agent reads in one prompt.
/// </summary>
/// <param name="Files">The files, with their content.</param>
public record AnalysisBatch(IReadOnlyList<BatchFile> Files)
{
    /// <summary>
    /// Gets the files by path with their line counts, which is what an answer is checked against.
    /// </summary>
    public IReadOnlyDictionary<string, int> LineCounts => Files.ToDictionary(_ => _.Path.Value, _ => _.Lines, StringComparer.OrdinalIgnoreCase);
}

/// <summary>
/// One file in a batch.
/// </summary>
/// <param name="Path">The path relative to the analyzed folder.</param>
/// <param name="Content">The content, possibly cut at the batch limit.</param>
public record BatchFile(SourcePath Path, string Content)
{
    /// <summary>
    /// Gets the number of lines in the content.
    /// </summary>
    public int Lines => Content.Split('\n').Length;
}
