// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace Cratis.Prologue.Extractor.Capturing.for_JsonFileCaptureOutput.when_writing_captures;

public class and_entries_exceed_the_limit : Specification
{
    string _directory = string.Empty;
    JsonFileCaptureOutput _output = null!;

    void Establish()
    {
        _directory = Path.Combine(Path.GetTempPath(), $"prologue-spec-{Guid.NewGuid():N}");
        _output = new JsonFileCaptureOutput(
            new JsonFileOptions { Directory = _directory, MaxEntriesPerFile = 3 },
            NullLogger<JsonFileCaptureOutput>.Instance);
    }

    async Task Because()
    {
        var occurred = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var entries = new List<Observation>();
        for (var i = 0; i < 4; i++)
        {
            entries.Add(new Observation(SourceKind.Http, occurred, new HttpCommandObserved("POST", $"/api/{i}", 201)));
        }

        entries.Add(new Observation(SourceKind.SqlServer, occurred, new DatabaseTransactionObserved("sqlserver", "Shop", "0x1", [])));

        await _output.Write([new Capture(Guid.NewGuid(), occurred, entries)]);
        _output.Dispose();
    }

    string PathFor(string kind, int index) => Path.Combine(_directory, JsonFileCaptureOutput.FileName(kind, index));

    [Fact] void should_write_the_first_http_file() => File.Exists(PathFor("http", 1)).ShouldBeTrue();
    [Fact] void should_roll_to_a_second_http_file() => File.Exists(PathFor("http", 2)).ShouldBeTrue();
    [Fact] void should_fill_the_first_http_file_to_the_limit() => File.ReadAllLines(PathFor("http", 1)).Length.ShouldEqual(3);
    [Fact] void should_put_the_overflow_in_the_second_file() => File.ReadAllLines(PathFor("http", 2)).Length.ShouldEqual(1);
    [Fact] void should_write_a_separate_file_per_source_kind() => File.Exists(PathFor("sqlserver", 1)).ShouldBeTrue();
    [Fact] void should_tag_records_with_the_capture_id() => File.ReadAllText(PathFor("sqlserver", 1)).ShouldContain("captureId");
}
#endif
