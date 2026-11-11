// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Discover.Extraction;

/// <summary>
/// Decides which files and folders are not source worth reading: build output, dependencies, generated code and whatever the
/// folder's own ".gitignore" names as a plain folder, file name or extension.
/// </summary>
public class IgnoreRules
{
    static readonly HashSet<string> _folders =
    [
        with(StringComparer.OrdinalIgnoreCase),
        "bin", "obj", "node_modules", ".git", ".vs", ".idea", ".vscode", "dist", "build", "out", "packages", ".next", ".nuget",
        "coverage", "TestResults", "vendor", "target", "__pycache__", ".venv", "venv", ".yarn", "wwwroot"
    ];

    static readonly HashSet<string> _extensions =
    [
        with(StringComparer.OrdinalIgnoreCase),
        ".cs", ".ts", ".tsx", ".js", ".jsx", ".mjs", ".java", ".kt", ".py", ".go", ".rb", ".php", ".rs", ".sql", ".yml", ".yaml", ".csproj"
    ];

    static readonly string[] _generatedSuffixes = [".g.cs", ".designer.cs", ".generated.cs", ".min.js", ".d.ts", ".Designer.cs"];

    readonly HashSet<string> _names = [with(StringComparer.OrdinalIgnoreCase)];
    readonly List<string> _ignoredSuffixes = [];

    /// <summary>
    /// Gets the rules that apply to every folder.
    /// </summary>
    public static IgnoreRules Default { get; } = new();

    /// <summary>
    /// Adds the plain entries of a ".gitignore": folder and file names, and "*.ext" patterns. Anything cleverer is left alone.
    /// </summary>
    /// <param name="gitignore">The content of the file.</param>
    /// <returns>The rules, including the defaults.</returns>
    public static IgnoreRules FromGitignore(string gitignore)
    {
        var rules = new IgnoreRules();
        foreach (var raw in gitignore.Split('\n'))
        {
            var line = raw.Trim();
            if (!IsEntry(line))
            {
                continue;
            }

            var pattern = line.Trim('/');
            if (pattern.StartsWith("*.", StringComparison.Ordinal) && IsPlain(pattern[1..]))
            {
                rules._ignoredSuffixes.Add(pattern[1..]);
            }
            else if (pattern.Length > 0 && IsPlain(pattern))
            {
                rules._names.Add(pattern);
            }
        }

        return rules;
    }

    /// <summary>
    /// Gets whether a folder is not entered.
    /// </summary>
    /// <param name="name">The folder's name.</param>
    /// <returns>True when it is skipped.</returns>
    public bool SkipsFolder(string name) => _folders.Contains(name) || _names.Contains(name) || (name.StartsWith('.') && name.Length > 1 && name != ".github");

    /// <summary>
    /// Gets whether a file is source worth reading.
    /// </summary>
    /// <param name="name">The file's name.</param>
    /// <returns>True when it is read.</returns>
    public bool Reads(string name)
    {
        return _extensions.Contains(Path.GetExtension(name)) && !_names.Contains(name) && !HasIgnoredSuffix(name);
    }

    static bool IsEntry(string line) => line.Length > 0 && !line.StartsWith('#') && !line.StartsWith('!');

    static bool IsPlain(string pattern) => pattern.IndexOfAny(['*', '?', '/', '[']) < 0;

    bool HasIgnoredSuffix(string name) =>
        _ignoredSuffixes.Exists(suffix => name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
        || _generatedSuffixes.Any(suffix => name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));
}
