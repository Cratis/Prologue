// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace Cratis.Prologue.Extractor.Capturing.for_JsonFileCaptureOutput.when_writing_captures;

public class and_captures_belong_to_a_named_service : Specification, IDisposable
{
    static readonly ServiceName _orders = "orders";
    static readonly DateTimeOffset _occurred = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    string _directory = string.Empty;
    JsonFileCaptureOutput _output = null!;
    Guid _ordersCaptureId;
    IReadOnlyList<Capture> _readBack = [];

    public void Dispose()
    {
        _output?.Dispose();

        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }

        GC.SuppressFinalize(this);
    }

    void Establish()
    {
        _directory = Path.Combine(Path.GetTempPath(), $"prologue-spec-{Guid.NewGuid():N}");
        _output = new JsonFileCaptureOutput(new JsonFileOptions { Directory = _directory }, NullLogger<JsonFileCaptureOutput>.Instance);
        _ordersCaptureId = Guid.NewGuid();
    }

    async Task Because()
    {
        var ordersCommand = new Observation(SourceKind.Http, _occurred, new HttpCommandObserved("POST", "/orders", 201)) { Service = _orders };
        var defaultCommand = new Observation(SourceKind.Http, _occurred, new HttpCommandObserved("POST", "/legacy", 201));

        await _output.Write(
        [
            new Capture(_ordersCaptureId, _occurred, [ordersCommand]) { Service = _orders },
            new Capture(Guid.NewGuid(), _occurred, [defaultCommand]),
        ]);
        _output.Dispose();

        _readBack = await CaptureFiles.ReadFromFolder(_directory);
    }

    string PathFor(ServiceName service, int index) => Path.Combine(_directory, JsonFileCaptureOutput.FileName(service, "http", index));

    [Fact] void should_write_the_named_service_to_its_own_file() => File.ReadAllLines(PathFor(_orders, 1)).Length.ShouldEqual(1);
    [Fact] void should_name_the_file_after_the_service() => Path.GetFileName(PathFor(_orders, 1)).ShouldEqual("prologue-orders-http-00001.jsonl");
    [Fact] void should_keep_the_default_service_file_name() => File.ReadAllLines(PathFor(ServiceName.Default, 1)).Length.ShouldEqual(1);
    [Fact] void should_read_the_named_service_back() => _readBack.Single(capture => capture.Id == _ordersCaptureId).Service.ShouldEqual(_orders);
    [Fact] void should_read_the_observations_service_back() => _readBack.Single(capture => capture.Id == _ordersCaptureId).Entries.Single().Service.ShouldEqual(_orders);
    [Fact] void should_read_the_default_service_back() => _readBack.Single(capture => capture.Id != _ordersCaptureId).Service.ShouldEqual(ServiceName.Default);
}
#endif
