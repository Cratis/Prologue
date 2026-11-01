// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Cratis.Prologue.Workbench.Captures.Recording.for_CaptureRecorder;

public class when_recording_fails : Specification
{
    ICommandPipeline _pipeline;
    ILogger<CaptureRecorder> _logger;
    Exception _exception;

    void Establish()
    {
        _pipeline = Substitute.For<ICommandPipeline>();
        _pipeline.Execute(Arg.Any<object>(), Arg.Any<ValidationResultSeverity?>()).Returns<Task<CommandResult>>(_ => throw new InvalidOperationException("the event store is down"));
        _logger = Substitute.For<ILogger<CaptureRecorder>>();
        _logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
    }

    async Task Because()
    {
        var at = DateTimeOffset.UtcNow;
        var capture = new Capture(Guid.NewGuid(), at, [new(SourceKind.Http, at, new HttpCommandObserved("POST", "/api/loans", 201))]);
        _exception = await Cratis.Specifications.Catch.Exception(() => new CaptureRecorder(_pipeline, _logger).Record(capture));
    }

    [Fact] void should_not_throw_so_the_stored_capture_is_still_acknowledged() => _exception.ShouldBeNull();
    [Fact] void should_log_the_failure_as_an_error() => _logger.ReceivedCalls().Count(call => call.GetMethodInfo().Name == nameof(ILogger.Log)).ShouldEqual(1);
}
#endif
