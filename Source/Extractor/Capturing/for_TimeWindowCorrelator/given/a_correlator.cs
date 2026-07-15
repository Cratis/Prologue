// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Configuration;
using Microsoft.Extensions.Options;

namespace Cratis.Prologue.Extractor.Capturing.for_TimeWindowCorrelator.given;

public class a_correlator : Specification
{
    protected TimeWindowCorrelator _correlator;
    protected readonly DateTimeOffset _origin = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    void Establish() => _correlator = new TimeWindowCorrelator(
        Options.Create(new PrologueOptions { Correlation = new CorrelationOptions { WindowMilliseconds = 1000 } }));

    protected static Observation Command(DateTimeOffset at, string traceId = "") =>
        new(SourceKind.Http, at, new HttpCommandObserved("POST", "/orders", 201, traceId));

    protected static Observation Transaction(DateTimeOffset at) =>
        new(SourceKind.SqlServer, at, new DatabaseTransactionObserved(
            "sqlserver", "Shop", "lsn", [new TableChange("dbo", "Orders", ChangeOperation.Insert, ["Id"])]));

    protected static Observation Span(DateTimeOffset at, string traceId, string name = "PlaceOrder") =>
        new(SourceKind.OpenTelemetry, at, new TelemetryObserved(
            traceId, "0000000000000001", string.Empty, name, "Server", "checkout", 1, 5, ["command.type"], new Dictionary<string, string>()));
}
#endif
