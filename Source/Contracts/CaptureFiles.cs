// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Prologue.Contracts;

/// <summary>
/// The canonical capture-file format — rolling JSON-lines files with one <see cref="CapturedEntry"/> per line,
/// partitioned per <see cref="SourceKind"/>. The Extractor writes this format; the Interpreter and any other
/// consumer read it back through this type, so all sides agree byte for byte.
/// </summary>
public static class CaptureFiles
{
    /// <summary>
    /// The file extension used for capture files.
    /// </summary>
    public const string Extension = ".jsonl";

    /// <summary>
    /// The search pattern matching every capture file in a folder. Any JSON-lines file counts — capture sets
    /// are not always produced by the Extractor's own rolling writer.
    /// </summary>
    public const string SearchPattern = "*" + Extension;

    /// <summary>
    /// Builds the file name for a given source kind and rolling index.
    /// </summary>
    /// <param name="sourceKind">The source kind the file holds entries for.</param>
    /// <param name="index">The rolling index.</param>
    /// <returns>The file name.</returns>
    public static string FileNameFor(string sourceKind, int index)
    {
        var safeKind = string.Concat(sourceKind.Select(character => char.IsLetterOrDigit(character) ? character : '-'));
        return $"prologue-{safeKind}-{index:D5}{Extension}";
    }

    /// <summary>
    /// Serializes a single <see cref="CapturedEntry"/> to its JSON-line representation.
    /// </summary>
    /// <param name="entry">The entry to serialize.</param>
    /// <returns>The JSON line.</returns>
    public static string Serialize(CapturedEntry entry) => JsonSerializer.Serialize(entry, CaptureSerialization.Options);

    /// <summary>
    /// Parses the content of a single capture file into its entries, skipping blank lines.
    /// </summary>
    /// <param name="content">The JSON-lines content of the file.</param>
    /// <returns>The parsed entries.</returns>
    public static IEnumerable<CapturedEntry> ParseEntries(string content) =>
        content.Split('\n')
            .Select(line => line.Trim())
            .Where(line => line.Length > 0)
            .Select(line => JsonSerializer.Deserialize<CapturedEntry>(line, CaptureSerialization.Options))
            .Where(entry => entry is not null)
            .Select(entry => entry!);

    /// <summary>
    /// Reconstructs correlated captures from the contents of one or more capture files, grouping entries by their
    /// capture id, ordering observations by occurrence and stamping each capture with the Prologue it belongs to.
    /// </summary>
    /// <param name="files">The contents of the capture files.</param>
    /// <param name="prologueId">The Prologue the captures belong to.</param>
    /// <returns>The reconstructed captures.</returns>
    public static IReadOnlyList<Capture> ToCaptures(IEnumerable<string> files, Guid prologueId = default) =>
    [
        .. files
            .SelectMany(ParseEntries)
            .GroupBy(entry => entry.CaptureId)
            .Select(group =>
            {
                var ordered = group.OrderBy(entry => entry.Occurred).ToList();
                return new Capture(group.Key, ordered[0].Occurred, [.. ordered.Select(entry => new Observation(entry.Source, entry.Occurred, entry.Payload))], prologueId);
            })
    ];

    /// <summary>
    /// Reads every capture file in a folder and reconstructs the correlated captures.
    /// </summary>
    /// <param name="folder">The folder holding the capture files.</param>
    /// <param name="prologueId">The Prologue the captures belong to.</param>
    /// <returns>The reconstructed captures.</returns>
    public static async Task<IReadOnlyList<Capture>> ReadFromFolder(string folder, Guid prologueId = default)
    {
        var files = new List<string>();
        foreach (var path in Directory.EnumerateFiles(folder, SearchPattern).Order())
        {
            files.Add(await File.ReadAllTextAsync(path));
        }

        return ToCaptures(files, prologueId);
    }
}
