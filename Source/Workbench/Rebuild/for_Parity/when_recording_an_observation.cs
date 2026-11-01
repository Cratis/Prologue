// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Captures;
using Cratis.Prologue.Workbench.Common.given;
using Cratis.Prologue.Workbench.Prologues;
using Cratis.Prologue.Workbench.Rebuild.Parity;

namespace Cratis.Prologue.Workbench.Rebuild.for_Parity;

public class when_recording_an_observation : Specification
{
    Guid _prologue;
    ParityObservation _reported;
    RecordParityObservation _mapped;
    RecordParityObservation _withoutShape;
    Result<ParityObserved, ValidationResult> _recorded;
    Result<ParityObserved, ValidationResult> _refused;

    void Because()
    {
        _prologue = Guid.NewGuid();
        _reported = new ParityObservation(Guid.NewGuid(), DateTimeOffset.UtcNow, "post", "/api/loans/4412/return?x=1", "POST /api/loans/{id}/return", "trace", 200, 409, ParityOutcome.Mismatched);
        _mapped = ParityMapping.ToCommand(_prologue, _reported);
        _withoutShape = ParityMapping.ToCommand(_prologue, _reported with { Shape = " " });
        _recorded = _mapped.Handle();
        _refused = (_mapped with { Shape = CommandShape.None }).Handle();
    }

    [Fact] void should_record_for_the_prologue_the_extractor_is_configured_for() => _mapped.Prologue.ShouldEqual(new PrologueId(_prologue));
    [Fact] void should_keep_the_identity_of_the_observation() => _mapped.Observation.Value.ShouldEqual(_reported.Id);
    [Fact] void should_normalize_the_method() => _mapped.Method.Value.ShouldEqual("POST");
    [Fact] void should_leave_the_query_string_out_of_the_path() => _mapped.Path.Value.ShouldEqual("/api/loans/4412/return");
    [Fact] void should_work_out_a_missing_shape() => _withoutShape.Shape.Value.ShouldEqual("POST /api/loans/{id}/return");
    [Fact] void should_record_the_legacy_status() => _recorded.Appended()!.LegacyStatus.Value.ShouldEqual(200);
    [Fact] void should_record_the_shadow_status() => _recorded.Appended()!.ShadowStatus.Value.ShouldEqual(409);
    [Fact] void should_record_the_outcome() => _recorded.Appended()!.Outcome.ShouldEqual(ParityOutcome.Mismatched);
    [Fact] void should_refuse_an_observation_with_no_command_shape() => _refused.Refusal().ShouldNotBeNull();
}
#endif
