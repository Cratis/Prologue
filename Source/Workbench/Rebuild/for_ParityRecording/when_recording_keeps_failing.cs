// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Common.Recording;
using Cratis.Prologue.Workbench.Rebuild.Parity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cratis.Prologue.Workbench.Rebuild.for_ParityRecording;

public class when_recording_keeps_failing : Specification
{
    int _flakyAttempts;
    int _brokenAttempts;
    bool _flakyReportedAgain;
    bool _brokenReportedAgain;
    ParityObservation _flaky;
    ParityObservation _broken;
    ParityRecordingQueue _queue;

    static ParityObservation Observation() => new(Guid.NewGuid(), DateTimeOffset.UtcNow, "POST", "/api/loans", "POST /api/loans", "trace", 200, 200, ParityOutcome.Matched);

    async Task Because()
    {
        _flaky = Observation();
        _broken = Observation();
        _queue = new ParityRecordingQueue(Microsoft.Extensions.Logging.Abstractions.NullLogger<ParityRecordingQueue>.Instance);
        _queue.TryEnqueue(Guid.Empty, _flaky);
        _queue.TryEnqueue(Guid.Empty, _broken);
        var recorder = Substitute.For<IParityRecorder>();
        recorder.Record(Arg.Any<Guid>(), Arg.Any<ParityObservation>(), Arg.Any<CancellationToken>()).Returns(call =>
        {
            if (call.Arg<ParityObservation>().Id == _flaky.Id)
            {
                return Task.FromResult(Interlocked.Increment(ref _flakyAttempts) >= 3);
            }

            Interlocked.Increment(ref _brokenAttempts);
            return Task.FromResult(false);
        });
        var services = new ServiceCollection().AddScoped(_ => recorder).BuildServiceProvider();
        var options = Options.Create(new RecordingOptions { ParityBackoff = TimeSpan.FromMilliseconds(5) });
        var worker = new ParityRecordingWorker(_queue, services.GetRequiredService<IServiceScopeFactory>(), options, Microsoft.Extensions.Logging.Abstractions.NullLogger<ParityRecordingWorker>.Instance);
        await worker.StartAsync(CancellationToken.None);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        while (Volatile.Read(ref _brokenAttempts) < 3 && !timeout.IsCancellationRequested)
        {
            await Task.Delay(10);
        }

        await Task.Delay(50);
        await worker.StopAsync(CancellationToken.None);
        _flakyReportedAgain = _queue.TryEnqueue(Guid.Empty, _flaky);
        _brokenReportedAgain = _queue.TryEnqueue(Guid.Empty, _broken);
    }

    [Fact] void should_try_again_until_it_is_recorded() => _flakyAttempts.ShouldEqual(3);
    [Fact] void should_give_up_after_the_bounded_number_of_attempts() => _brokenAttempts.ShouldEqual(3);
    [Fact] void should_remember_the_id_of_what_was_recorded() => _flakyReportedAgain.ShouldBeTrue();
    [Fact] void should_forget_the_id_of_what_was_dropped() => _brokenReportedAgain.ShouldBeTrue();
}
#endif
