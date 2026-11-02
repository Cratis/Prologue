// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Events.Boundaries;
using Cratis.Prologue.Workbench.Events.given;
using Cratis.Prologue.Workbench.Events.NotEvents;
using Cratis.Prologue.Workbench.Events.Questions;
using Cratis.Prologue.Workbench.Events.Suggestions;
using Cratis.Prologue.Workbench.Events.Uncaptured;

namespace Cratis.Prologue.Workbench.Events.Interpreting.for_InterpretationDecision;

public class when_questions_are_raised : an_uncaptured_library
{
    IEnumerable<object> _first;
    IEnumerable<object> _again;
    IEnumerable<object> _afterAnswering;

    void Because()
    {
        var (all, model, questions) = UncapturedEvents.Extend(_suggestions, _model, _uncaptured, []);
        var boundaries = BoundaryDetection.Detect(_facts, _scope);
        var notEvents = NotEventDetection.Detect(_facts, _scope);
        _first = InterpretationDecision.Decide(null, all, model, boundaries, notEvents, questions);
        var review = EventReview.Reduce(new RaisedQuestionsRecorded(questions), _review, _context);
        review = EventReview.Reduce(new EventSuggestionsInterpreted(all), review, _context);
        review = EventReview.Reduce(new InterpretedModelRecorded(model), review, _context);
        review = EventReview.Reduce(new InterpretationFindingsRecorded(boundaries, notEvents), review, _context);
        _again = InterpretationDecision.Decide(review, all, model, boundaries, notEvents, questions);
        _afterAnswering = InterpretationDecision.Decide(review, all, model, boundaries, notEvents, []);
    }

    [Fact] void should_record_the_questions() => _first.OfType<RaisedQuestionsRecorded>().Single().Questions.Count().ShouldEqual(1);
    [Fact] void should_record_nothing_when_the_same_evidence_raises_the_same_questions() => _again.ShouldBeEmpty();
    [Fact] void should_record_that_no_question_is_raised_anymore() => _afterAnswering.OfType<RaisedQuestionsRecorded>().Single().Questions.ShouldBeEmpty();
}
#endif
