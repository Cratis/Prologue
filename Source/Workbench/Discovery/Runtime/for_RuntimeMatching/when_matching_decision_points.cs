// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Workbench.Discovery.Runtime.for_RuntimeMatching;

public class when_matching_decision_points : Specification
{
    static readonly RuntimeEvidence _evidence = new(
        [
            new(new RouteShape("POST /api/loans"), [new(200, 902), new(409, 72), new(422, 3)]),
            new(new RouteShape("POST /api/authors"), [new(200, 12)])
        ],
        14);

    DecisionMatch _refusal;
    DecisionMatch _neverRefused;
    DecisionMatch _stateChange;
    DecisionMatch _unknownShape;
    DecisionMatch _noShape;
    DecisionMatch _schedule;
    DecisionMatch _scheduleWithoutWrites;

    void Because()
    {
        _refusal = RuntimeMatching.Match(DecisionKind.Refusal, new RouteShape("POST /api/loans"), _evidence);
        _neverRefused = RuntimeMatching.Match(DecisionKind.Refusal, new RouteShape("POST /api/authors"), _evidence);
        _stateChange = RuntimeMatching.Match(DecisionKind.StateChange, new RouteShape("POST /api/loans"), _evidence);
        _unknownShape = RuntimeMatching.Match(DecisionKind.Refusal, new RouteShape("POST /api/loans/{id}/renew"), _evidence);
        _noShape = RuntimeMatching.Match(DecisionKind.Refusal, RouteShape.None, _evidence);
        _schedule = RuntimeMatching.Match(DecisionKind.Schedule, RouteShape.None, _evidence);
        _scheduleWithoutWrites = RuntimeMatching.Match(DecisionKind.Schedule, RouteShape.None, RuntimeEvidence.None);
    }

    [Fact] void should_count_refusals_as_every_client_error() => _refusal.Happened.ShouldEqual(75);
    [Fact] void should_name_the_status_refusals_happened_with_most() => _refusal.Status.ShouldEqual(409);
    [Fact] void should_match_a_refusal_that_happened() => _refusal.Runtime.ShouldEqual(DecisionRuntime.Matches);
    [Fact] void should_flag_a_refusal_that_never_happened_as_code_only() => _neverRefused.Runtime.ShouldEqual(DecisionRuntime.CodeOnly);
    [Fact] void should_count_state_changes_as_the_command_being_accepted() => _stateChange.Happened.ShouldEqual(902);
    [Fact] void should_flag_a_command_never_captured_as_code_only() => _unknownShape.Runtime.ShouldEqual(DecisionRuntime.CodeOnly);
    [Fact] void should_flag_a_decision_without_a_shape_as_code_only() => _noShape.Runtime.ShouldEqual(DecisionRuntime.CodeOnly);
    [Fact] void should_let_a_schedule_explain_uncorrelated_writes() => (_schedule.Runtime, _schedule.Happened).ShouldEqual((DecisionRuntime.ExplainsWrites, 14));
    [Fact] void should_flag_a_schedule_with_no_such_writes_as_code_only() => _scheduleWithoutWrites.Runtime.ShouldEqual(DecisionRuntime.CodeOnly);
}
#endif
