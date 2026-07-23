// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Extractor.Capturing.for_TimeWindowCorrelator.given;

namespace Cratis.Prologue.Extractor.Capturing.for_TimeWindowCorrelator.when_draining;

public class and_a_database_schema_is_observed : a_correlator
{
    IReadOnlyList<Capture> _result;

    void Establish() => _correlator.Add(Schema(_origin));

    void Because() => _result = _correlator.Drain(_origin + TimeSpan.FromSeconds(5));

    [Fact] void should_produce_a_standalone_capture() => _result.Count.ShouldEqual(1);
    [Fact] void should_contain_only_the_schema() => _result[0].Entries.Count.ShouldEqual(1);
    [Fact] void should_carry_the_schema_payload() => _result[0].Entries[0].Payload.ShouldBeOfExactType<DatabaseSchemaObserved>();
}
#endif
