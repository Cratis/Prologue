// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Events.Boundaries;
using Cratis.Prologue.Workbench.Events.given;
using Cratis.Prologue.Workbench.Events.NotEvents;
using Cratis.Prologue.Workbench.Events.Suggestions;

namespace Cratis.Prologue.Workbench.Events.Interpreting.for_InterpretationDecision;

public class when_the_evidence_changed : a_library_interpretation
{
    IEnumerable<object> _events;
    IEnumerable<object> _withdrawn;

    void Because()
    {
        var boundaries = BoundaryDetection.Detect(_facts, _scope);
        var notEvents = NotEventDetection.Detect(_facts, _scope);
        var review = EventReview.Reduce(new InterpretationFindingsRecorded(boundaries, notEvents), _review, _context);
        var changed = _suggestions.Select((suggestion, index) => index == 0 ? suggestion with { Confidence = 0.11 } : suggestion).ToList();
        _events = InterpretationDecision.Decide(review, changed, _model, boundaries, notEvents, []);
        _withdrawn = InterpretationDecision.Decide(review, _suggestions.Skip(1), _model, boundaries, notEvents, []);
    }

    [Fact] void should_record_the_suggestions() => _events.OfType<EventSuggestionsInterpreted>().Count().ShouldEqual(1);
    [Fact] void should_not_record_the_structure_again() => _events.OfType<InterpretedModelRecorded>().ShouldBeEmpty();
    [Fact] void should_not_record_the_findings_again() => _events.OfType<InterpretationFindingsRecorded>().ShouldBeEmpty();
    [Fact] void should_record_the_suggestions_when_one_is_gone() => _withdrawn.OfType<EventSuggestionsInterpreted>().Single().Suggestions.Count().ShouldEqual(_suggestions.Count - 1);
}
#endif
