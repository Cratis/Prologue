// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Screenplay;
using Cratis.Prologue.Workbench.Events;
using Cratis.Prologue.Workbench.Events.Accepted;
using Cratis.Prologue.Workbench.Events.given;
using Cratis.Prologue.Workbench.Events.Reviewing;
using Cratis.Prologue.Workbench.Events.Suggestions;
using Cratis.Screenplay;
using Cratis.Screenplay.Diagnostics;
using Cratis.Screenplay.Printing;
using Cratis.Screenplay.Syntax;

namespace Cratis.Prologue.Workbench.Model.Export.for_ModelScreenplays;

public class when_generating_the_play_of_a_model : a_library_interpretation
{
    static readonly SuggestionKey _author = "Authors/AuthorCreated";
    ModelScreenplay _mixed;
    ModelScreenplay _decided;
    ModelScreenplay _nothing;
    CompilationResult<ApplicationSyntax> _compilation;

    void Because()
    {
        var generator = new ScreenplayGenerator(new ScreenplayPrinter());
        var review = EventReview.Reduce(new EventSuggestionAccepted(_author), _review, _context);
        review = EventReview.Reduce(new EventSuggestionRenamed(_author, "AuthorRegistered"), review, _context);
        _mixed = ModelScreenplays.From(AcceptedModels.From(_prologue, review), generator);
        _compilation = new ScreenplayCompiler().Compile(_mixed.Play);

        var everything = _review.Suggestions.Aggregate(_review, (current, suggestion) => EventReview.Reduce(new EventSuggestionAccepted(suggestion.Suggestion.Key), current, _context));
        _decided = ModelScreenplays.From(AcceptedModels.From(_prologue, everything), generator);
        _nothing = ModelScreenplays.From(AcceptedModels.From(_prologue, null), generator);
    }

    [Fact] void should_compile_without_errors() => _compilation.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
    [Fact] void should_compile_without_warnings() => _compilation.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Warning).ShouldBeEmpty();
    [Fact] void should_produce_an_application() => _compilation.Value.ShouldNotBeNull();
    [Fact] void should_name_the_accepted_event_as_it_was_renamed() => _mixed.Play.ShouldContain("AuthorRegistered");
    [Fact] void should_mark_the_slices_with_open_suggestions_as_drafts() => _mixed.Play.ShouldContain(ModelScreenplays.DraftMarker);
    [Fact] void should_count_the_drafts() => _mixed.Drafts.ShouldBeGreaterThan(0);
    [Fact] void should_mark_nothing_once_everything_is_accepted() => _decided.Play.ShouldNotContain(ModelScreenplays.DraftMarker);
    [Fact] void should_count_no_drafts_once_everything_is_accepted() => _decided.Drafts.ShouldEqual(0);
    [Fact] void should_compile_a_fully_accepted_model() => new ScreenplayCompiler().Compile(_decided.Play).Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
    [Fact] void should_generate_an_empty_play_when_nothing_was_interpreted() => _nothing.Play.ShouldBeEmpty();
}
#endif
