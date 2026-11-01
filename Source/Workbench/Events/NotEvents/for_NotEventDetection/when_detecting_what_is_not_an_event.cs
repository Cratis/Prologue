// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Events.given;

namespace Cratis.Prologue.Workbench.Events.NotEvents.for_NotEventDetection;

public class when_detecting_what_is_not_an_event : a_library_interpretation
{
    IReadOnlyList<NotEvent> _notEvents;

    void Because() => _notEvents = NotEventDetection.Detect(_facts, _scope);

    NotEvent Of(string subject) => _notEvents.Single(notEvent => notEvent.Subject == subject);

    [Fact] void should_call_a_column_every_command_keeps_up_to_date_derived_state() => Of("Inventory.AvailableCount").Kind.ShouldEqual(NotEventKind.DerivedState);
    [Fact] void should_say_which_commands_change_it() => Of("Inventory.AvailableCount").Reason.ShouldContain("POST /api/loans/{id}");
    [Fact] void should_call_a_nullable_column_set_later_a_fact_of_its_own() => Of("Loans.ReturnedOn").Kind.ShouldEqual(NotEventKind.NullableFact);
    [Fact] void should_list_the_excluded_traffic() => Of("POST /api/simulation/start").Kind.ShouldEqual(NotEventKind.ExcludedTraffic);
    [Fact] void should_not_call_a_column_the_command_is_about_derived() => _notEvents.Any(notEvent => notEvent.Subject == "Authors.Name").ShouldBeFalse();
    [Fact] void should_not_call_a_key_nullable() => _notEvents.Any(notEvent => notEvent.Subject == "Loans.LoanId").ShouldBeFalse();
    [Fact] void should_list_the_derived_state_first() => _notEvents[0].Kind.ShouldEqual(NotEventKind.DerivedState);
    [Fact] void should_list_a_derived_column_once() => _notEvents.Count(notEvent => notEvent.Subject == "Inventory.AvailableCount").ShouldEqual(1);
    [Fact] void should_leave_out_a_shape_that_is_excluded() => NotEventDetection.Detect(_facts, null).Any(notEvent => notEvent.Kind == NotEventKind.ExcludedTraffic).ShouldBeFalse();
}
#endif
