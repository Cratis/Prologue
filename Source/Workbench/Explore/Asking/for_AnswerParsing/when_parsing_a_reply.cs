// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Captures;

namespace Cratis.Prologue.Workbench.Explore.Asking.for_AnswerParsing;

public class when_parsing_a_reply : Specification
{
    static readonly CommandShape[] _known = ["POST /api/loans", "POST /api/loans/{id}/return", "DELETE /api/authors/{id}"];
    ParsedAnswer _plain;
    ParsedAnswer _fenced;
    ParsedAnswer _invented;
    ParsedAnswer _loose;
    ParsedAnswer _prose;
    ParsedAnswer _noAnswer;
    ParsedAnswer _broken;

    void Because()
    {
        _plain = AnswerParsing.Parse("""{"answer": "Lending inserts a loan.", "citations": ["POST /api/loans"]}""", _known);
        _fenced = AnswerParsing.Parse("Here you go:\n```json\n{\"answer\": \"It is refused.\", \"citations\": [\"DELETE /api/authors/{id}\", \"POST /api/loans/{id}/return\"]}\n```", _known);
        _invented = AnswerParsing.Parse("""{"answer": "Maybe.", "citations": ["POST /api/loans", "POST /api/made/up", "GET /api/loans"]}""", _known);
        _loose = AnswerParsing.Parse("""{"Answer": "Same route twice.", "Citations": ["post /api/loans", "POST  /api/loans", "POST /api/loans"]}""", _known);
        _prose = AnswerParsing.Parse("I think lending works fine.", _known);
        _noAnswer = AnswerParsing.Parse("""{"citations": ["POST /api/loans"]}""", _known);
        _broken = AnswerParsing.Parse("{ not json }", _known);
    }

    [Fact] void should_read_the_answer() => _plain.Text.ShouldEqual("Lending inserts a loan.");
    [Fact] void should_read_the_citations() => _plain.Citations.Select(shape => shape.Value).ShouldContainOnly(["POST /api/loans"]);
    [Fact] void should_find_the_object_inside_a_code_fence() => _fenced.Citations.Count().ShouldEqual(2);
    [Fact] void should_drop_citations_of_shapes_that_do_not_exist() => _invented.Citations.Select(shape => shape.Value).ShouldContainOnly(["POST /api/loans"]);
    [Fact] void should_keep_the_answer_when_a_citation_is_dropped() => _invented.Text.ShouldEqual("Maybe.");
    [Fact] void should_match_a_citation_without_regard_to_case_and_spacing_and_cite_each_shape_once() => _loose.Citations.Select(shape => shape.Value).ShouldContainOnly(["POST /api/loans"]);
    [Fact] void should_give_no_answer_for_prose() => _prose.ShouldBeNull();
    [Fact] void should_give_no_answer_without_an_answer_property() => _noAnswer.ShouldBeNull();
    [Fact] void should_give_no_answer_for_broken_json() => _broken.ShouldBeNull();
}
#endif
