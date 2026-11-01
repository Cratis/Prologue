// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Captures.given;
using Cratis.Prologue.Workbench.Common.given;
using Cratis.Prologue.Workbench.Prologues;
using Cratis.Prologue.Workbench.Rebuild.Drift;

namespace Cratis.Prologue.Workbench.Rebuild.for_ModelDrift;

public class when_acknowledging_drift : Specification
{
    PrologueId _prologue;
    EventContext _context;
    ModelDrift _drift;
    Result<DriftAcknowledged, ValidationResult> _acknowledged;
    Result<DriftAcknowledged, ValidationResult> _again;
    Result<DriftAcknowledged, ValidationResult> _unknown;
    Result<ModelAcceptedForRebuild, ValidationResult> _firstAcceptance;
    Result<ModelAcceptedForRebuild, ValidationResult> _secondAcceptance;

    void Because()
    {
        _prologue = PrologueId.New();
        _context = EventContext.EmptyWithEventSourceId(_prologue);
        _firstAcceptance = new AcceptModelForRebuild(_prologue).Handle(null);
        _drift = ModelDrift.Reduce(_firstAcceptance.Appended()!, null, _context);
        _secondAcceptance = new AcceptModelForRebuild(_prologue).Handle(_drift);
        _drift = ModelDrift.Reduce(RecordedCaptures.Command("PUT", "/api/members/1", 200), _drift, _context);

        var key = _drift.Items.Single().Key;
        _acknowledged = new AcknowledgeDrift(_prologue, key).Handle(_drift);
        var after = ModelDrift.Reduce(_acknowledged.Appended()!, _drift, _context);
        _again = new AcknowledgeDrift(_prologue, key).Handle(after);
        _unknown = new AcknowledgeDrift(_prologue, "shape|GET /nothing").Handle(_drift);
        _drift = after;
    }

    [Fact] void should_accept_the_model_once() => _firstAcceptance.Appended().ShouldNotBeNull();
    [Fact] void should_refuse_accepting_it_again() => _secondAcceptance.Refusal().ShouldNotBeNull();
    [Fact] void should_acknowledge_known_drift() => _acknowledged.Appended()!.Shape.Value.ShouldEqual("PUT /api/members/{id}");
    [Fact] void should_mark_the_drift_acknowledged() => _drift.Items.Single().Acknowledged.ShouldBeTrue();
    [Fact] void should_refuse_acknowledging_twice() => _again.Refusal().ShouldNotBeNull();
    [Fact] void should_refuse_drift_that_is_not_known() => _unknown.Refusal().ShouldNotBeNull();
}
#endif
