// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Workbench.Events.Uncaptured.for_PastTense;

public class when_putting_verbs_in_the_past : Specification
{
    [Fact] void should_add_ed() => PastTense.Of("return").ShouldEqual("Returned");
    [Fact] void should_add_d_after_an_e() => PastTense.Of("approve").ShouldEqual("Approved");
    [Fact] void should_turn_y_into_ied() => PastTense.Of("notify").ShouldEqual("Notified");
    [Fact] void should_keep_y_after_a_vowel() => PastTense.Of("play").ShouldEqual("Played");
    [Fact] void should_double_the_last_letter_of_ship() => PastTense.Of("ship").ShouldEqual("Shipped");
    [Fact] void should_know_irregular_verbs() => PastTense.Of("lend").ShouldEqual("Lent");
    [Fact] void should_not_change_a_verb_already_in_the_past() => PastTense.Of("Returned").ShouldEqual("Returned");
    [Fact] void should_be_empty_for_nothing() => PastTense.Of(" ").ShouldBeEmpty();
}
#endif
