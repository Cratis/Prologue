// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Extractor.Shadowing.for_ShadowComparison;

public class when_planning_the_body : Specification
{
    [Fact] void should_copy_nothing_without_a_body() => BodyPlans.For(null, false).ShouldEqual(BodyPlan.None);
    [Fact] void should_copy_nothing_for_an_empty_body() => BodyPlans.For(0, false).ShouldEqual(BodyPlan.None);
    [Fact] void should_buffer_a_small_body() => BodyPlans.For(2048, false).ShouldEqual(BodyPlan.Buffer);
    [Fact] void should_buffer_a_body_of_exactly_the_limit() => BodyPlans.For(ShadowLimits.MaximumBodyBytes, false).ShouldEqual(BodyPlan.Buffer);
    [Fact] void should_skip_a_body_over_the_limit() => BodyPlans.For(ShadowLimits.MaximumBodyBytes + 1, false).ShouldEqual(BodyPlan.Skip);
    [Fact] void should_skip_a_body_whose_size_is_not_known() => BodyPlans.For(null, true).ShouldEqual(BodyPlan.Skip);
}
#endif
