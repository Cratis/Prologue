// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Extractor.Capturing;

namespace Cratis.Prologue.Extractor.Sources.Postgres.for_PgTransactionAccumulator.when_completing;

public class and_no_tables_changed : Specification
{
    readonly PgTransactionAccumulator _accumulator = new();
    Observation? _result;

    void Establish() => _accumulator.Begin("4242");

    void Because() => _result = _accumulator.Complete("shop", DateTimeOffset.UtcNow);

    [Fact] void should_not_produce_an_observation() => _result.ShouldBeNull();
}
#endif
