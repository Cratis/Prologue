// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Workbench.Events.Uncaptured.for_CodeOnlyNames;

public class when_deriving_names : Specification
{
    [Fact] void should_name_a_post_to_a_collection_after_creating_the_entity() => CodeOnlyNames.From("POST /api/reservations").ShouldEqual("ReservationCreated");
    [Fact] void should_name_a_put_after_updating_the_entity() => CodeOnlyNames.From("PUT /api/members/{id}").ShouldEqual("MemberUpdated");
    [Fact] void should_name_a_delete_after_deleting_the_entity() => CodeOnlyNames.From("DELETE /api/members/{id}").ShouldEqual("MemberDeleted");
    [Fact] void should_name_an_action_after_the_entity_and_what_it_did() => CodeOnlyNames.From("POST /api/loans/{id}/renew").ShouldEqual("LoanRenewed");
    [Fact] void should_name_an_action_without_an_id_the_same_way() => CodeOnlyNames.From("POST /api/books/return").ShouldEqual("BookReturned");
    [Fact] void should_use_the_irregular_past_tense() => CodeOnlyNames.From("POST /api/books/{id}/lend").ShouldEqual("BookLent");
    [Fact] void should_keep_the_rest_of_a_hyphenated_action() => CodeOnlyNames.From("POST /api/loans/{id}/mark-overdue").ShouldEqual("LoanOverdueMarked");
    [Fact] void should_treat_a_sub_resource_as_the_entity() => CodeOnlyNames.From("POST /api/loans/{id}/fines").ShouldEqual("FineCreated");
    [Fact] void should_skip_the_version_segment() => CodeOnlyNames.From("POST /api/v2/reservations").ShouldEqual("ReservationCreated");
    [Fact] void should_name_nothing_for_a_query() => CodeOnlyNames.From("GET /api/loans").ShouldBeEmpty();
    [Fact] void should_name_nothing_for_a_shape_without_a_path() => CodeOnlyNames.From("POST /").ShouldBeEmpty();
    [Fact] void should_name_nothing_for_text_that_is_not_a_shape() => CodeOnlyNames.From("reservations").ShouldBeEmpty();
    [Fact] void should_name_a_capability_without_a_route_after_what_it_does() => CodeOnlyNames.FromCapability("Overdue sweep").ShouldEqual("OverdueSweepCompleted");
    [Fact] void should_name_nothing_for_a_capability_without_a_name() => CodeOnlyNames.FromCapability("  ").ShouldBeEmpty();
}
#endif
