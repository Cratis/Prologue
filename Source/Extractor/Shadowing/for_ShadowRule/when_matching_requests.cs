// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Configuration;
using Cratis.Prologue.Extractor.Shadowing.given;

namespace Cratis.Prologue.Extractor.Shadowing.for_ShadowRule;

public class when_matching_requests : Specification
{
    ShadowRule _byShape;
    ShadowRule _byRoute;
    ShadowRule _both;

    void Establish()
    {
        _byShape = Rules.For("post /api/loans/{id}/return");
        _byRoute = Rules.For(string.Empty, "/api/loans/{id}/return");
        _both = Rules.For("POST /api/loans/{id}/return", "/api/loans/{id:int}/return");
    }

    [Fact] void should_match_a_request_of_the_shape_whatever_the_identity() => _byShape.Matches("POST", "/api/loans/4412/return").ShouldBeTrue();
    [Fact] void should_ignore_the_case_of_the_method_in_the_shape() => _byShape.Matches("post", "/api/loans/4412/return").ShouldBeTrue();
    [Fact] void should_not_match_another_method() => _byShape.Matches("PUT", "/api/loans/4412/return").ShouldBeFalse();
    [Fact] void should_not_match_another_path() => _byShape.Matches("POST", "/api/loans/4412").ShouldBeFalse();
    [Fact] void should_never_mirror_a_read() => Rules.For("GET /api/loans").Matches("GET", "/api/loans").ShouldBeFalse();
    [Fact] void should_match_a_route_pattern() => _byRoute.Matches("POST", "/api/loans/9/return").ShouldBeTrue();
    [Fact] void should_not_match_a_path_the_route_does_not_describe() => _byRoute.Matches("POST", "/api/loans/9/extend").ShouldBeFalse();
    [Fact] void should_match_a_route_for_any_command_method() => _byRoute.Matches("DELETE", "/api/loans/9/return").ShouldBeTrue();
    [Fact] void should_require_both_when_a_rule_names_both() => _both.Matches("POST", "/api/loans/abc/return").ShouldBeFalse();
    [Fact] void should_match_when_both_agree() => _both.Matches("POST", "/api/loans/5/return").ShouldBeTrue();
    [Fact] void should_refuse_a_rule_that_names_nothing_to_match() => Assert.Throws<InvalidOperationException>(() => new ShadowRule(new ShadowRuleOptions { Destination = "http://x/" }));
    [Fact] void should_refuse_a_destination_that_is_not_an_address() => Assert.Throws<InvalidOperationException>(() => new ShadowRule(new ShadowRuleOptions { Shape = "POST /a", Destination = "new-system" }));
}
#endif
