// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Extractor.Shadowing.given;
using Microsoft.Extensions.Logging.Abstractions;

namespace Cratis.Prologue.Extractor.Shadowing.for_ShadowDispatcher;

public class when_the_queue_is_full : Specification
{
    int _queued;
    bool _next;

    void Because()
    {
        var dispatcher = new ShadowDispatcher(Substitute.For<IShadowSender>(), Substitute.For<IParityReporter>(), NullLogger<ShadowDispatcher>.Instance);
        var job = new ShadowJob(Guid.NewGuid(), Rules.For("POST /api/loans"), DateTimeOffset.UtcNow, "POST", "/api/loans", string.Empty, "POST /api/loans", string.Empty, [], null, 201, false);
        while (dispatcher.TryEnqueue(job))
        {
            _queued++;
        }

        _next = dispatcher.TryEnqueue(job);
    }

    [Fact] void should_hold_no_more_than_the_capacity() => _queued.ShouldEqual(ShadowLimits.QueueCapacity);
    [Fact] void should_refuse_instead_of_waiting() => _next.ShouldBeFalse();
}
#endif
