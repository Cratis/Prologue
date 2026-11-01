// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Workbench.Discover.Extraction.for_RouteTemplate;

public class when_building_shapes : Specification
{
    RouteShape _typedId;
    RouteShape _isbn;
    RouteShape _plain;
    RouteShape _tokens;
    RouteShape _absolute;
    RouteShape _optional;
    RouteShape _numericLiteral;
    RouteShape _runtimePath;

    void Because()
    {
        _typedId = RouteTemplate.Shape("post", "api/loans", "{id:int}/return", "Loans", "ReturnLoan");
        _isbn = RouteTemplate.Shape("POST", "api/catalog/books", "{isbn}/tags", "Catalog", "Tag");
        _plain = RouteTemplate.Shape("POST", "api/loans", null, "Loans", "Lend");
        _tokens = RouteTemplate.Shape("DELETE", "api/[controller]", "[action]/{id:guid}", "Authors", "Remove");
        _absolute = RouteTemplate.Shape("PUT", "api/loans", "/api/members/{memberId}/loans/", "Loans", "Move");
        _optional = RouteTemplate.Shape("POST", "/api/loans/", "{id?}/renew", "Loans", "Renew");
        _numericLiteral = RouteTemplate.Shape("POST", "api/loans", "2649/return", "Loans", "Return");
        _runtimePath = RouteTemplate.Normalize("post", "/api/loans/2649/return?x=1");
    }

    [Fact] void should_replace_a_typed_route_parameter() => _typedId.Value.ShouldEqual("POST /api/loans/{id}/return");
    [Fact] void should_replace_a_named_route_parameter() => _isbn.Value.ShouldEqual("POST /api/catalog/books/{id}/tags");
    [Fact] void should_use_the_controller_route_when_the_action_has_no_template() => _plain.Value.ShouldEqual("POST /api/loans");
    [Fact] void should_fill_the_controller_and_action_tokens() => _tokens.Value.ShouldEqual("DELETE /api/authors/remove/{id}");
    [Fact] void should_let_an_absolute_action_route_replace_the_controller_route() => _absolute.Value.ShouldEqual("PUT /api/members/{id}/loans");
    [Fact] void should_treat_an_optional_parameter_like_any_other() => _optional.Value.ShouldEqual("POST /api/loans/{id}/renew");
    [Fact] void should_replace_a_numeric_literal_the_way_captures_do() => _numericLiteral.Value.ShouldEqual("POST /api/loans/{id}/return");
    [Fact] void should_match_the_shape_a_captured_path_gets() => _runtimePath.Value.ShouldEqual(_typedId.Value);
}
#endif
