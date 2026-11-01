// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Events.Boundaries;
using Cratis.Prologue.Workbench.Events.given;
using Cratis.Prologue.Workbench.Events.Interpreting;
using Cratis.Prologue.Workbench.Events.Reviewing;
using Cratis.Prologue.Workbench.Events.Suggestions;

namespace Cratis.Prologue.Workbench.Events.Accepted.for_AcceptedModels;

public class when_building_from_a_review : a_library_interpretation
{
    static readonly SuggestionKey _author = "Authors/AuthorCreated";
    static readonly SuggestionKey _delete = "Authors/AuthorDeleted";
    static readonly SuggestionKey _lend = "Loans/LoanCreated";
    AcceptedModel _model;
    AcceptedModel _untouched;
    AcceptedModel _nothing;

    void Because()
    {
        var review = EventReview.Reduce(new InterpretationFindingsRecorded(BoundaryDetection.Detect(_facts, _scope), []), _review, _context);
        review = EventReview.Reduce(new EventSuggestionAccepted(_author), review, _context);
        review = EventReview.Reduce(new EventSuggestionRenamed(_author, "AuthorRegistered"), review, _context);
        review = EventReview.Reduce(new EventSuggestionRejected(_delete), review, _context);
        review = EventReview.Reduce(new ConsistencyBoundaryChosen("POST /api/loans/{id}|across", BoundaryDetection.OneStream, "Loan is the consistency boundary of lending"), review, _context);
        _model = AcceptedModels.From(_prologue, review);
        _untouched = AcceptedModels.From(_prologue, _review);
        _nothing = AcceptedModels.From(_prologue, null);
    }

    IEnumerable<AcceptedSlice> Slices(AcceptedModel model) => model.Modules.SelectMany(module => module.Features).SelectMany(feature => feature.Slices);

    IEnumerable<AcceptedEvent> Events(AcceptedModel model) => Slices(model).SelectMany(slice => slice.Events);

    [Fact] void should_use_the_name_a_person_chose() => Events(_model).Single(@event => @event.Key == _author).Name.Value.ShouldEqual("AuthorRegistered");
    [Fact] void should_keep_the_mechanical_name_beside_it() => Events(_model).Single(@event => @event.Key == _author).Mechanical.Value.ShouldEqual("AuthorCreated");
    [Fact] void should_not_mark_an_accepted_event_open() => Events(_model).Single(@event => @event.Key == _author).IsOpen.ShouldBeFalse();
    [Fact] void should_leave_a_rejected_event_out() => Events(_model).Any(@event => @event.Key == _delete).ShouldBeFalse();
    [Fact] void should_leave_a_slice_out_whose_events_were_all_rejected() => Slices(_model).Any(slice => slice.Commands.Any(command => command.Name.Value == "DeleteAuthor")).ShouldBeFalse();
    [Fact] void should_keep_an_event_nobody_decided_on_marked_open() => Events(_model).Single(@event => @event.Key == _lend).IsOpen.ShouldBeTrue();
    [Fact] void should_mark_a_slice_with_an_open_event_open() => Slices(_model).Single(slice => slice.Events.Any(@event => @event.Key == _lend)).IsOpen.ShouldBeTrue();
    [Fact] void should_count_the_accepted_events() => _model.Accepted.ShouldEqual(1);
    [Fact] void should_count_the_open_events() => _model.Open.ShouldEqual(_review.Suggestions.Count() - 2);
    [Fact] void should_count_the_rejected_events() => _model.Rejected.ShouldEqual(1);
    [Fact] void should_count_the_boundaries_nobody_decided() => _model.OpenBoundaries.ShouldEqual(1);
    [Fact] void should_give_a_slice_the_command_shapes_its_events_were_captured_from() => Slices(_model).Single(slice => slice.Events.Any(@event => @event.Key == _author)).Shapes.Single().Value.ShouldEqual("POST /api/authors/register");
    [Fact] void should_add_the_choice_made_for_a_boundary_to_the_slices_it_concerns() => Slices(_model).Single(slice => slice.Events.Any(@event => @event.Key == _lend)).Rules.Any(rule => rule.Kind == AcceptedRuleKind.Boundary && rule.Text.Contains("Loan is the consistency boundary")).ShouldBeTrue();
    [Fact] void should_add_the_choice_to_the_rules_of_the_model() => _model.Rules.Any(rule => rule.Kind == AcceptedRuleKind.Boundary).ShouldBeTrue();
    [Fact] void should_add_the_constraints_the_evidence_suggests() => _model.Rules.Any(rule => rule.Kind == AcceptedRuleKind.Constraint && rule.Text.Contains("Isbn")).ShouldBeTrue();
    [Fact] void should_add_the_rules_the_code_holds() => _model.Rules.Any(rule => rule.Kind == AcceptedRuleKind.Rule && rule.Text.Contains("Member has 5 open loans")).ShouldBeTrue();
    [Fact] void should_keep_everything_open_when_nothing_was_decided() => Events(_untouched).All(@event => @event.IsOpen).ShouldBeTrue();
    [Fact] void should_have_no_modules_when_nothing_was_interpreted() => _nothing.Modules.ShouldBeEmpty();
    [Fact] void should_group_the_slices_by_module() => _model.Modules.Select(module => module.Name.Value).ShouldContain("Authors");
}
#endif
