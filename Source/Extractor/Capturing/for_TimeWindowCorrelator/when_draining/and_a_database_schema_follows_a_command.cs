// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Extractor.Capturing.for_TimeWindowCorrelator.given;

namespace Cratis.Prologue.Extractor.Capturing.for_TimeWindowCorrelator.when_draining;

public class and_a_database_schema_follows_a_command : a_correlator
{
    IReadOnlyList<Capture> _result;

    void Establish()
    {
        _correlator.Add(Command(_origin));
        _correlator.Add(Schema(_origin + TimeSpan.FromMilliseconds(100)));
    }

    void Because() => _result = _correlator.Drain(_origin + TimeSpan.FromSeconds(5));

    [Fact] void should_produce_two_captures() => _result.Count.ShouldEqual(2);
    [Fact] void should_not_claim_the_schema_as_evidence_of_the_command() => _result[0].Entries.Count.ShouldEqual(1);
    [Fact] void should_keep_the_schema_in_its_own_capture() => _result[1].Entries[0].Payload.ShouldBeOfExactType<DatabaseSchemaObserved>();
}
#endif
