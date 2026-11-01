// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Workbench.Explore.Journeys.for_PathIdentities;

public class when_finding_identities_in_a_path : Specification
{
    PathIdentity[] _loan;
    PathIdentity[] _book;
    PathIdentity[] _two;
    PathIdentity[] _none;
    PathIdentity[] _guid;

    void Because()
    {
        _loan = [.. PathIdentities.From("/api/loans/2649/return")];
        _book = [.. PathIdentities.From("/api/catalog/books/978-0-123456/tags")];
        _two = [.. PathIdentities.From("/api/members/7/Loans/55?expand=true")];
        _none = [.. PathIdentities.From("/api/loans")];
        _guid = [.. PathIdentities.From("/api/orders/3f2504e0-4f89-11d3-9a0c-0305e82c3301")];
    }

    [Fact] void should_name_what_a_numeric_segment_identifies_by_the_segment_before_it() => (_loan.Single().Kind.Value, _loan.Single().Id.Value).ShouldEqual(("loans", "2649"));
    [Fact] void should_find_identifiers_that_hold_digits_like_an_isbn() => (_book.Single().Kind.Value, _book.Single().Id.Value).ShouldEqual(("books", "978-0-123456"));
    [Fact] void should_find_every_identity_in_the_order_they_appear() => _two.Select(identity => (identity.Kind.Value, identity.Id.Value)).ShouldContainOnly([("members", "7"), ("loans", "55")]);
    [Fact] void should_ignore_the_query_string() => _two.Length.ShouldEqual(2);
    [Fact] void should_find_a_guid() => _guid.Single().Kind.Value.ShouldEqual("orders");
    [Fact] void should_find_nothing_in_a_path_without_an_identity() => _none.ShouldBeEmpty();
}
#endif
