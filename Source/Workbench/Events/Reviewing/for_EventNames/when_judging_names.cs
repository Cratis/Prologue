// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Workbench.Events.Reviewing.for_EventNames;

public class when_judging_names : Specification
{
    [Fact] void should_accept_pascal_case() => EventNames.IsPascalCase("BookLentToMember").ShouldBeTrue();
    [Fact] void should_accept_digits() => EventNames.IsPascalCase("Book2Lent").ShouldBeTrue();
    [Fact] void should_reject_camel_case() => EventNames.IsPascalCase("bookLent").ShouldBeFalse();
    [Fact] void should_reject_spaces() => EventNames.IsPascalCase("Book Lent").ShouldBeFalse();
    [Fact] void should_reject_underscores() => EventNames.IsPascalCase("Book_Lent").ShouldBeFalse();
    [Fact] void should_reject_nothing() => EventNames.IsPascalCase(string.Empty).ShouldBeFalse();
    [Fact] void should_not_advise_about_a_regular_past_tense() => EventNames.Advice("AuthorRegistered").ShouldBeEmpty();
    [Fact] void should_not_advise_about_an_irregular_past_tense() => EventNames.Advice("BookLent").ShouldBeEmpty();
    [Fact] void should_advise_about_a_noun() => EventNames.Advice("AuthorRegistration").ShouldNotBeEmpty();
    [Fact] void should_not_advise_about_a_mechanical_name() => EventNames.Advice("InventoryUpdated").ShouldBeEmpty();
    [Fact] void should_advise_about_an_imperative() => EventNames.Advice("RegisterAuthor").ShouldNotBeEmpty();
}
#endif
