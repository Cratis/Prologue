// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Discovery.Extraction;

/// <summary>
/// Lists the source files under a folder, skipping build output, dependencies and anything too large to read.
/// </summary>
public static class SourceFileScanner
{
    /// <summary>
    /// The largest file read, in bytes. A larger file is generated or data, not code to understand.
    /// </summary>
    public const long MaxFileSize = 200_000;

    /// <summary>
    /// The most files an analysis reads, so a huge repository cannot run for ever.
    /// </summary>
    public const int MaxFiles = 3000;

    /// <summary>
    /// Scans a folder.
    /// </summary>
    /// <param name="root">The folder.</param>
    /// <returns>The files in path order; none when the folder does not exist.</returns>
    public static IReadOnlyList<SourceFile> Scan(string root)
    {
        if (!Directory.Exists(root))
        {
            return [];
        }

        var gitignore = Path.Combine(root, ".gitignore");
        var rules = File.Exists(gitignore) ? IgnoreRules.FromGitignore(File.ReadAllText(gitignore)) : IgnoreRules.Default;
        var files = new List<SourceFile>();
        Walk(new DirectoryInfo(root), string.Empty, rules, files);
        return [.. files.OrderBy(_ => _.Path.Value, StringComparer.Ordinal).Take(MaxFiles)];
    }

    static void Walk(DirectoryInfo directory, string prefix, IgnoreRules rules, List<SourceFile> files)
    {
        foreach (var file in directory.EnumerateFiles().Where(_ => !_.Attributes.HasFlag(FileAttributes.ReparsePoint)))
        {
            if (rules.Reads(file.Name) && file.Length is > 0 and <= MaxFileSize)
            {
                files.Add(new SourceFile(new SourcePath(prefix + file.Name), file.Length));
            }
        }

        foreach (var child in directory.EnumerateDirectories().Where(_ => !_.Attributes.HasFlag(FileAttributes.ReparsePoint) && !rules.SkipsFolder(_.Name)))
        {
            Walk(child, $"{prefix}{child.Name}/", rules, files);
        }
    }
}
