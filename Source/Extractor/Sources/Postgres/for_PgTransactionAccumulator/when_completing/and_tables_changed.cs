// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Extractor.Capturing;

namespace Cratis.Prologue.Extractor.Sources.Postgres.for_PgTransactionAccumulator.when_completing;

public class and_tables_changed : Specification
{
    readonly PgTransactionAccumulator _accumulator = new();
    readonly DateTimeOffset _committed = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    Observation? _result;

    void Establish()
    {
        _accumulator.Begin("4242");
        _accumulator.Record("public", "orders", ChangeOperation.Insert, ["id", "total"]);
    }

    void Because() => _result = _accumulator.Complete("shop", _committed);

    [Fact] void should_produce_an_observation() => _result.ShouldNotBeNull();
    [Fact] void should_carry_the_transaction_id() => ((DatabaseTransactionObserved)_result.Payload).TransactionId.ShouldEqual("4242");
    [Fact] void should_anchor_to_the_commit_time() => _result.Occurred.ShouldEqual(_committed);
}
#endif
