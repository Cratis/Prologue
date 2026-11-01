// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Captures.given;
using Cratis.Prologue.Workbench.Captures.Recording;
using Cratis.Prologue.Workbench.Decisions;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Captures.Live.for_RecentCaptures;

public class when_more_captures_arrive_than_are_kept : Specification
{
    PrologueId _prologue;
    RecentCaptures _recent;
    CaptureRecorded _newest;

    void Because()
    {
        _prologue = PrologueId.New();
        var context = EventContext.EmptyWithEventSourceId(_prologue);
        for (var index = 0; index < RecentCaptures.Limit + 5; index++)
        {
            _recent = RecentCaptures.Reduce(RecordedCaptures.Command("POST", "/api/loans", 201), _recent, context);
        }

        _newest = RecordedCaptures.Command("POST", "/api/loans/1/return", 200);
        _recent = RecentCaptures.Reduce(_newest, _recent, context);
    }

    [Fact] void should_belong_to_the_prologue() => _recent.Id.ShouldEqual(_prologue);
    [Fact] void should_keep_only_the_limit() => _recent.Captures.Count().ShouldEqual(RecentCaptures.Limit);
    [Fact] void should_show_the_newest_first() => _recent.Captures.First().Capture.ShouldEqual(_newest.Capture);
    [Fact] void should_classify_each_capture() => _recent.Captures.First().Outcome.ShouldEqual(Decisions.DecisionOutcome.Accepted);
}
#endif
