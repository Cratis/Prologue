// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Workbench.Explore.Observations.for_Share;

public class when_taking_a_percentage : Specification
{
    [Fact] void should_round_to_a_whole_percentage() => Share.Percent(1, 3).ShouldEqual(33);
    [Fact] void should_round_half_up() => Share.Percent(1, 8).ShouldEqual(13);
    [Fact] void should_never_show_something_that_happened_as_zero() => Share.Percent(1, 1000).ShouldEqual(1);
    [Fact] void should_show_nothing_as_zero() => Share.Percent(0, 10).ShouldEqual(0);
    [Fact] void should_show_everything_as_one_hundred() => Share.Percent(7, 7).ShouldEqual(100);
    [Fact] void should_not_divide_by_zero() => Share.Percent(3, 0).ShouldEqual(0);
}
#endif
