// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Extractor.Capturing.for_ServiceObservationChannel;

public class when_publishing : Specification
{
    ObservationChannel _inner;
    ServiceObservationChannel _channel;
    Observation _published;

    void Establish()
    {
        _inner = new ObservationChannel();
        _channel = new ServiceObservationChannel(_inner, "orders");
    }

    async Task Because()
    {
        await _channel.Publish(new Observation(
            SourceKind.SqlServer,
            DateTimeOffset.UnixEpoch,
            new DatabaseTransactionObserved("sqlserver", "Orders", "0x1", [])));

        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await foreach (var observation in _inner.ReadAll(cancellation.Token))
        {
            _published = observation;
            break;
        }
    }

    [Fact] void should_attribute_the_observation_to_the_service() => _published.Service.ShouldEqual(new ServiceName("orders"));
    [Fact] void should_hand_on_the_payload_unchanged() => _published.Payload.ShouldBeOfExactType<DatabaseTransactionObserved>();
}
#endif
