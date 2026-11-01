// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Configuration;

namespace Cratis.Prologue.Extractor.Capturing.for_CaptureServices.when_resolving;

public class and_two_services_claim_the_same_cluster : Specification
{
    Exception _result;

    void Because() => _result = Catch.Exception(() => CaptureServices.From(new PrologueOptions
    {
        Services = new Dictionary<string, ServiceOptions>
        {
            ["orders"] = new() { Clusters = ["api"] },
            ["billing"] = new() { Clusters = ["api"] },
        },
    }));

    [Fact] void should_refuse_the_configuration() => _result.ShouldBeOfExactType<AmbiguousCaptureService>();
}
#endif
