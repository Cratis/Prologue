// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Extractor.Capturing.for_TimeWindowCorrelator.given;

namespace Cratis.Prologue.Extractor.Capturing.for_TimeWindowCorrelator.when_draining;

/// <summary>
/// Two services working at the same moment must not have their evidence mixed: a transaction in the billing
/// database is never evidence of a command sent to the orders service, however close together they happened.
/// </summary>
public class and_observations_belong_to_different_services : a_correlator
{
    static readonly ServiceName _orders = "orders";
    static readonly ServiceName _billing = "billing";

    IReadOnlyList<Capture> _result;

    void Establish()
    {
        _correlator.Add(Command(_origin) with { Service = _orders });
        _correlator.Add(Transaction(_origin + TimeSpan.FromMilliseconds(100)) with { Service = _billing });
        _correlator.Add(Transaction(_origin + TimeSpan.FromMilliseconds(200)) with { Service = _orders });
    }

    void Because() => _result = _correlator.Drain(_origin + TimeSpan.FromSeconds(5));

    Capture OrdersCapture => _result.Single(capture => capture.Service == _orders);
    Capture BillingCapture => _result.Single(capture => capture.Service == _billing);

    [Fact] void should_produce_a_capture_per_service() => _result.Count.ShouldEqual(2);
    [Fact] void should_correlate_the_command_with_its_own_services_transaction() => OrdersCapture.Entries.Count.ShouldEqual(2);
    [Fact] void should_keep_the_other_services_transaction_on_its_own() => BillingCapture.Entries.Single().Service.ShouldEqual(_billing);
    [Fact] void should_stamp_the_capture_with_the_prologue() => OrdersCapture.PrologueId.ShouldEqual(_prologueId);
}
#endif
