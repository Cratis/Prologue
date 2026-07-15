// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Cratis.Prologue.Configuration;

namespace Cratis.Prologue.Extractor.Capturing;

/// <summary>
/// Represents an <see cref="ICaptureOutput"/> that appends captured observations to rolling JSON-lines files,
/// partitioned per <see cref="SourceKind"/>. Each file holds at most <see cref="JsonFileOptions.MaxEntriesPerFile"/>
/// entries before rolling to the next index. Only the single output worker calls <see cref="Write"/>, so no locking
/// is required.
/// </summary>
public sealed class JsonFileCaptureOutput : ICaptureOutput, IDisposable
{
    readonly JsonFileOptions _options;
    readonly ILogger<JsonFileCaptureOutput> _logger;
    readonly Dictionary<string, RollingWriter> _writers = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonFileCaptureOutput"/> class.
    /// </summary>
    /// <param name="options">The JSON file output options.</param>
    /// <param name="logger">The logger.</param>
    public JsonFileCaptureOutput(JsonFileOptions options, ILogger<JsonFileCaptureOutput> logger)
    {
        _options = options;
        _logger = logger;
        Directory.CreateDirectory(_options.Directory);
    }

    /// <summary>
    /// Builds the file name for a given source kind and rolling index.
    /// </summary>
    /// <param name="sourceKind">The source kind the file holds entries for.</param>
    /// <param name="index">The rolling index.</param>
    /// <returns>The file name.</returns>
    public static string FileName(string sourceKind, int index) => CaptureFiles.FileNameFor(sourceKind, index);

    /// <inheritdoc/>
    public Task Write(IReadOnlyList<Capture> captures, CancellationToken cancellationToken = default)
    {
        foreach (var capture in captures)
        {
            foreach (var entry in capture.Entries)
            {
                var writer = WriterFor(entry.Source.Value);
                var record = new CapturedEntry(capture.Id, entry.Occurred, entry.Source, entry.Payload);
                writer.WriteLine(CaptureFiles.Serialize(record), _options.MaxEntriesPerFile);
            }
        }

        foreach (var writer in _writers.Values)
        {
            writer.Flush();
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        foreach (var writer in _writers.Values)
        {
            writer.Dispose();
        }

        _writers.Clear();
    }

    RollingWriter WriterFor(string sourceKind)
    {
        if (!_writers.TryGetValue(sourceKind, out var writer))
        {
            writer = new RollingWriter(_options.Directory, sourceKind, NextIndexFor(sourceKind), _logger);
            _writers[sourceKind] = writer;
        }

        return writer;
    }

    int NextIndexFor(string sourceKind)
    {
        var safeKind = string.Concat(sourceKind.Select(character => char.IsLetterOrDigit(character) ? character : '-'));
        var existing = Directory.EnumerateFiles(_options.Directory, $"prologue-{safeKind}-*.jsonl")
            .Select(path => Path.GetFileNameWithoutExtension(path).Split('-').LastOrDefault())
            .Select(suffix => int.TryParse(suffix, out var value) ? value : 0)
            .DefaultIfEmpty(0)
            .Max();
        return existing + 1;
    }

    sealed class RollingWriter(string directory, string sourceKind, int startIndex, ILogger logger) : IDisposable
    {
        StreamWriter? _writer;
        int _index = startIndex;
        int _count;

        public void WriteLine(string line, int maxEntriesPerFile)
        {
            if (_writer is null || _count >= maxEntriesPerFile)
            {
                Roll();
            }

            _writer!.WriteLine(line);
            _count++;
        }

        public void Flush() => _writer?.Flush();

        public void Dispose()
        {
            _writer?.Flush();
            _writer?.Dispose();
            _writer = null;
        }

        void Roll()
        {
            _writer?.Flush();
            _writer?.Dispose();
            var path = Path.Combine(directory, FileName(sourceKind, _index));
            _writer = new StreamWriter(path, append: true) { AutoFlush = false };
            _count = 0;
            _index++;
            JsonFileCaptureOutputLog.RolledToFile(logger, path);
        }
    }
}
