// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Discovery.Extraction;

/// <summary>
/// Groups files into batches an agent can read in one prompt. Files in the same folder stay together.
/// </summary>
public static class AnalysisBatches
{
    /// <summary>
    /// The most bytes of source in one batch.
    /// </summary>
    public const long MaxBatchSize = 60_000;

    /// <summary>
    /// The most files in one batch.
    /// </summary>
    public const int MaxBatchFiles = 25;

    /// <summary>
    /// Splits files into batches, keeping their order.
    /// </summary>
    /// <param name="files">The files, in path order.</param>
    /// <returns>The batches.</returns>
    public static IReadOnlyList<IReadOnlyList<SourceFile>> Split(IEnumerable<SourceFile> files)
    {
        var batches = new List<IReadOnlyList<SourceFile>>();
        var current = new List<SourceFile>();
        var size = 0L;

        foreach (var file in files)
        {
            if (current.Count > 0 && (current.Count >= MaxBatchFiles || size + file.Size > MaxBatchSize))
            {
                batches.Add(current);
                current = [];
                size = 0;
            }

            current.Add(file);
            size += file.Size;
        }

        if (current.Count > 0)
        {
            batches.Add(current);
        }

        return batches;
    }
}
