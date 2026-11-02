// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Rebuild.Parity;

namespace Cratis.Prologue.Workbench.Rebuild.for_ParityRecording;

public class when_the_event_store_is_unavailable : Specification
{
    bool _recorded;

    async Task Because()
    {
        var pipeline = Substitute.For<ICommandPipeline>();
        pipeline.Execute(Arg.Any<object>(), Arg.Any<ValidationResultSeverity?>()).Returns<Task<CommandResult>>(_ => throw new InvalidOperationException("Chronicle is down"));
        var observation = new ParityObservation(Guid.NewGuid(), DateTimeOffset.UtcNow, "POST", "/api/loans", "POST /api/loans", "trace", 200, 200, ParityOutcome.Matched);
        _recorded = await new ParityRecorder(pipeline, Microsoft.Extensions.Logging.Abstractions.NullLogger<ParityRecorder>.Instance).Record(Guid.NewGuid(), observation);
    }

    [Fact] void should_tell_the_worker_it_failed_so_it_can_try_again() => _recorded.ShouldBeFalse();
}
#endif
