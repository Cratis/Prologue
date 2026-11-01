// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Workbench.Events.Interpreting.for_SuggestionConfidence;

public class when_working_out_the_confidence : Specification
{
    [Fact] void should_be_low_without_captures() => SuggestionConfidence.For(0, 0, false).Value.ShouldEqual(0.3);
    [Fact] void should_grow_with_the_captures() => SuggestionConfidence.For(20, 20, false).Value.ShouldBeGreaterThan(SuggestionConfidence.For(2, 2, false).Value);
    [Fact] void should_drop_when_the_event_shows_up_in_only_some_of_the_captures() => SuggestionConfidence.For(5, 50, false).Value.ShouldBeLessThan(SuggestionConfidence.For(5, 5, false).Value);
    [Fact] void should_gain_a_little_when_the_code_confirms_the_command() => SuggestionConfidence.For(5, 5, true).Value.ShouldBeGreaterThan(SuggestionConfidence.For(5, 5, false).Value);
    [Fact] void should_never_be_certain() => SuggestionConfidence.For(100000, 100000, true).Value.ShouldBeLessThan(1.0);
}
#endif
