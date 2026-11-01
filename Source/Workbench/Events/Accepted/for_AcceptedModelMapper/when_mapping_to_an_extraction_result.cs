// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Interpreter.Contracts;
using Cratis.Prologue.Workbench.Events.Boundaries;
using Cratis.Prologue.Workbench.Events.given;
using Cratis.Prologue.Workbench.Events.Reviewing;
using Cratis.Prologue.Workbench.Events.Suggestions;

namespace Cratis.Prologue.Workbench.Events.Accepted.for_AcceptedModelMapper;

public class when_mapping_to_an_extraction_result : a_library_interpretation
{
    static readonly SuggestionKey _author = "Authors/AuthorCreated";
    static readonly SuggestionKey _book = "Books/BookCreated";
    ExtractionResult _withDrafts;
    ExtractionResult _withoutDrafts;

    void Because()
    {
        var review = EventReview.Reduce(new EventSuggestionAccepted(_author), _review, _context);
        review = EventReview.Reduce(new EventSuggestionRenamed(_author, "AuthorRegistered"), review, _context);
        review = EventReview.Reduce(new EventSuggestionAccepted(_book), review, _context);
        review = EventReview.Reduce(new ConsistencyBoundaryChosen("POST /api/loans/{id}|across", "one-stream", "Loan is the consistency boundary of lending"), review with { Boundaries = BoundaryDetection.Detect(_facts, _scope) }, _context);
        var model = AcceptedModels.From(_prologue, review);
        _withDrafts = AcceptedModelMapper.ToExtractionResult(model);
        _withoutDrafts = AcceptedModelMapper.ToExtractionResult(model, includeOpen: false);
    }

    static IEnumerable<ExtractedSlice> Slices(ExtractionResult result) => result.Modules.SelectMany(module => module.Features).SelectMany(feature => feature.Slices);

    [Fact] void should_belong_to_the_prologue() => _withDrafts.PrologueId.ShouldEqual(_prologue.Value);
    [Fact] void should_use_the_final_event_name() => Slices(_withDrafts).SelectMany(slice => slice.Events).Select(@event => @event.Name).ShouldContain("AuthorRegistered");
    [Fact] void should_not_use_the_mechanical_name_of_a_renamed_event() => Slices(_withDrafts).SelectMany(slice => slice.Events).Select(@event => @event.Name).ShouldNotContain("AuthorCreated");
    [Fact] void should_carry_the_event_properties() => Slices(_withDrafts).SelectMany(slice => slice.Events).Single(@event => @event.Name == "AuthorRegistered").Properties.Select(property => property.Name).ShouldContain("Name");
    [Fact] void should_carry_the_commands() => Slices(_withDrafts).SelectMany(slice => slice.Commands).Select(command => command.Name).ShouldContain("RegisterAuthor");
    [Fact] void should_carry_the_read_models() => Slices(_withDrafts).SelectMany(slice => slice.ReadModels).Any().ShouldBeTrue();
    [Fact] void should_point_projections_at_the_final_names() => Slices(_withDrafts).SelectMany(slice => slice.Projections).SelectMany(projection => projection.SourceEvents).ShouldContain("AuthorRegistered");
    [Fact] void should_carry_the_constraints_of_the_events_it_keeps() => Slices(_withDrafts).SelectMany(slice => slice.Constraints).Any(constraint => constraint.Property == "isbn").ShouldBeTrue();
    [Fact] void should_write_the_rules_into_the_description_of_the_slice() => Slices(_withDrafts).Any(slice => slice.Description.Contains("Rule: Loan is the consistency boundary of lending")).ShouldBeTrue();
    [Fact] void should_keep_events_nobody_decided_on_as_drafts() => Slices(_withDrafts).SelectMany(slice => slice.Events).Select(@event => @event.Name).ShouldContain("LoanCreated");
    [Fact] void should_leave_the_drafts_out_when_asked_to() => Slices(_withoutDrafts).SelectMany(slice => slice.Events).Select(@event => @event.Name).ShouldContainOnly("AuthorRegistered", "BookCreated");
    [Fact] void should_leave_a_slice_out_that_has_nothing_left_when_drafts_are_left_out() => Slices(_withoutDrafts).Any(slice => slice.Commands.Any(command => command.Name == "CreateLoan")).ShouldBeFalse();
    [Fact] void should_leave_the_draft_events_of_a_module_out() => Slices(_withoutDrafts).SelectMany(slice => slice.Events).Any(@event => @event.Name == "LoanCreated").ShouldBeFalse();
}
#endif
