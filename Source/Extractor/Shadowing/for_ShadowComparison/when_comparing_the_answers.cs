// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Extractor.Shadowing.for_ShadowComparison;

public class when_comparing_the_answers : Specification
{
    [Fact] void should_match_when_both_accepted_with_the_same_status() => ShadowComparison.Compare(201, 201).ShouldEqual(ParityOutcome.Matched);
    [Fact] void should_match_when_both_accepted_with_different_statuses() => ShadowComparison.Compare(200, 204).ShouldEqual(ParityOutcome.Matched);
    [Fact] void should_treat_a_redirect_as_accepting() => ShadowComparison.Compare(302, 201).ShouldEqual(ParityOutcome.Matched);
    [Fact] void should_match_when_both_refused_for_different_reasons() => ShadowComparison.Compare(409, 400).ShouldEqual(ParityOutcome.Matched);
    [Fact] void should_not_match_when_legacy_accepted_and_the_shadow_refused() => ShadowComparison.Compare(201, 409).ShouldEqual(ParityOutcome.Mismatched);
    [Fact] void should_not_match_when_legacy_refused_and_the_shadow_accepted() => ShadowComparison.Compare(409, 201).ShouldEqual(ParityOutcome.Mismatched);
    [Fact] void should_treat_a_server_error_as_refusing() => ShadowComparison.Compare(201, 500).ShouldEqual(ParityOutcome.Mismatched);
    [Fact] void should_be_unavailable_when_the_shadow_did_not_answer() => ShadowComparison.Compare(201, null).ShouldEqual(ParityOutcome.Unavailable);
    [Fact] void should_be_unavailable_when_the_shadow_is_a_gateway_error() => new[] { 502, 503, 504 }.Select(status => ShadowComparison.Compare(201, status)).ShouldContainOnly([ParityOutcome.Unavailable, ParityOutcome.Unavailable, ParityOutcome.Unavailable]);
    [Fact] void should_be_unavailable_when_the_shadow_answer_decides_nothing() => ShadowComparison.Compare(201, 101).ShouldEqual(ParityOutcome.Unavailable);
    [Fact] void should_compare_only_answers_that_decide() => new[] { ShadowComparison.IsDecision(201), ShadowComparison.IsDecision(404), ShadowComparison.IsDecision(101), ShadowComparison.IsDecision(0) }.ShouldContainOnly([true, true, false, false]);
}
#endif
