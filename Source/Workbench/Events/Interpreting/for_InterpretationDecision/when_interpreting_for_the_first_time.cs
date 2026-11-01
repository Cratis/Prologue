// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Events.Boundaries;
using Cratis.Prologue.Workbench.Events.given;
using Cratis.Prologue.Workbench.Events.NotEvents;
using Cratis.Prologue.Workbench.Events.Suggestions;

namespace Cratis.Prologue.Workbench.Events.Interpreting.for_InterpretationDecision;


public class when_interpreting_for_the_first_time : a_library_interpretation
{
    IEnumerable<object> _events;

    void Because() => _events = InterpretationDecision.Decide(null, _suggestions, _model, BoundaryDetection.Detect(_facts, _scope), NotEventDetection.Detect(_facts, _scope));

    [Fact] void should_record_the_suggestions() => _events.OfType<EventSuggestionsInterpreted>().Single().Suggestions.Count().ShouldEqual(_suggestions.Count);
    [Fact] void should_record_the_structure() => _events.OfType<InterpretedModelRecorded>().Count().ShouldEqual(1);
    [Fact] void should_record_the_findings() => _events.OfType<InterpretationFindingsRecorded>().Count().ShouldEqual(1);
}

#endif
