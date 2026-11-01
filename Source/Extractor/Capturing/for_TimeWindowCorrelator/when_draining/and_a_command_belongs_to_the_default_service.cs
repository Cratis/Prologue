// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Extractor.Capturing.for_TimeWindowCorrelator.given;

namespace Cratis.Prologue.Extractor.Capturing.for_TimeWindowCorrelator.when_draining;

public class and_a_command_belongs_to_the_default_service : a_correlator
{
    IReadOnlyList<Capture> _result;

    void Establish()
    {
        _correlator.Add(Command(_origin));
        _correlator.Add(Transaction(_origin + TimeSpan.FromMilliseconds(200)));
    }

    void Because() => _result = _correlator.Drain(_origin + TimeSpan.FromSeconds(5));

    [Fact] void should_attribute_the_capture_to_the_default_service() => _result.Single().Service.ShouldEqual(ServiceName.Default);
}
#endif
