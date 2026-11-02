// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Common.given;
using Cratis.Prologue.Workbench.Prologues;
using Cratis.Prologue.Workbench.Rebuild.Parity;

namespace Cratis.Prologue.Workbench.Rebuild.for_Parity;

public class when_an_observation_is_reported_again : Specification
{
    RecordParityObservation _command;
    IReadOnlyList<ParityObserved> _first;
    RecentParity _recent;
    Result<IEnumerable<ParityObserved>, ValidationResult> _retry;
    Result<IEnumerable<ParityObserved>, ValidationResult> _other;

    void Because()
    {
        var reported = new ParityObservation(Guid.NewGuid(), DateTimeOffset.UtcNow, "POST", "/api/loans", "POST /api/loans", "trace", 200, 200, ParityOutcome.Matched);
        _command = ParityMapping.ToCommand(Guid.NewGuid(), reported);
        _first = _command.Handle(null).Events();
        _recent = new RecentParity(_command.Prologue, _first);
        _retry = _command.Handle(_recent);
        _other = (_command with { Observation = new ObservationId(Guid.NewGuid()) }).Handle(_recent);
    }

    [Fact] void should_record_the_first_report() => _first.Count.ShouldEqual(1);
    [Fact] void should_record_nothing_for_the_retry() => _retry.Events().ShouldBeEmpty();
    [Fact] void should_not_refuse_the_retry() => _retry.Refusal().ShouldBeNull();
    [Fact] void should_still_record_a_different_observation() => _other.Events().Count.ShouldEqual(1);
}
#endif
