// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Events.Boundaries;
using Cratis.Prologue.Workbench.Events.given;
using Cratis.Prologue.Workbench.Events.NotEvents;
using Cratis.Prologue.Workbench.Events.Suggestions;

namespace Cratis.Prologue.Workbench.Events.Interpreting.for_InterpretationDecision;

public class when_interpreting_the_same_evidence_again : a_library_interpretation
{
    IEnumerable<object> _events;

    void Because()
    {
        var boundaries = BoundaryDetection.Detect(_facts, _scope);
        var notEvents = NotEventDetection.Detect(_facts, _scope);
        var review = EventReview.Reduce(new InterpretationFindingsRecorded(boundaries, notEvents), _review, _context);
        _events = InterpretationDecision.Decide(review, _suggestions, _model, boundaries, notEvents);
    }

    [Fact] void should_record_nothing() => _events.ShouldBeEmpty();
}
#endif
