// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Runtime.CompilerServices;
using Cratis.Prologue.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Cratis.Prologue.Extractor.Capturing.for_CorrelationWorker;

public class when_storing_a_capture_throws : Specification
{
    readonly List<Guid> _stored = [];

    Capture _first;
    Capture _second;
    Exception? _escaped;
    bool _workerStillRunning;

    static async IAsyncEnumerable<Observation> Silence([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await Task.Delay(Timeout.Infinite, cancellationToken);
        yield break;
    }

    async Task Because()
    {
        _first = new Capture(Guid.NewGuid(), DateTimeOffset.UtcNow, []);
        _second = new Capture(Guid.NewGuid(), DateTimeOffset.UtcNow, []);

        var channel = Substitute.For<IObservationChannel>();
        channel.ReadAll(Arg.Any<CancellationToken>()).Returns(call => Silence(call.Arg<CancellationToken>()));

        var drained = 0;
        var correlator = Substitute.For<ICorrelator>();
        correlator.Drain(Arg.Any<DateTimeOffset>()).Returns(_ => drained++ == 0 ? [_first, _second] : []);

        var store = Substitute.For<ICaptureStore>();
        store.Store(Arg.Any<Capture>(), Arg.Any<CancellationToken>()).Returns(call =>
        {
            var capture = call.Arg<Capture>();
            if (capture.Id == _first.Id)
            {
                throw new InvalidOperationException("the output is unavailable");
            }

            _stored.Add(capture.Id);
            return Task.CompletedTask;
        });

        var options = Options.Create(new PrologueOptions { Correlation = new CorrelationOptions { WindowMilliseconds = 100 } });
        var worker = new CorrelationWorker(channel, correlator, store, options, NullLogger<CorrelationWorker>.Instance);
        await worker.StartAsync(CancellationToken.None);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        while (_stored.Count == 0 && !timeout.IsCancellationRequested)
        {
            await Task.Delay(10);
        }

        _escaped = worker.ExecuteTask?.Exception;
        _workerStillRunning = worker.ExecuteTask is { IsCompleted: false };
        await worker.StopAsync(CancellationToken.None);
    }

    [Fact] void should_not_let_the_exception_escape_the_worker() => _escaped.ShouldBeNull();
    [Fact] void should_keep_the_worker_running() => _workerStillRunning.ShouldBeTrue();
    [Fact] void should_still_store_the_next_capture() => _stored.ShouldContainOnly(_second.Id);
}
#endif
