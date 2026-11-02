// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Workbench.AI.Network.for_NetworkPositions;

// Mirrors Source/Workbench/AI/for_networkPosture: the dispatcher and the shell must agree on which providers are inside.
public class when_deciding_whether_a_provider_is_inside : Specification
{
    [Theory]
    [InlineData("http://localhost:11434/v1", true)]
    [InlineData("127.0.0.1:8000", true)]
    [InlineData("http://10.1.2.3", true)]
    [InlineData("http://172.20.0.5", true)]
    [InlineData("http://172.32.0.5", false)]
    [InlineData("http://192.168.1.4", true)]
    [InlineData("http://llm.corp/v1", true)]
    [InlineData("http://gpu-box", true)]
    [InlineData("https://api.openai.com/v1", false)]
    [InlineData("https://models.example.com", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void should_recognize_a_private_endpoint(string? endpoint, bool expected) => NetworkPositions.IsPrivateEndpoint(endpoint).ShouldEqual(expected);

    [Fact] void should_let_a_mark_win_over_the_endpoint() => NetworkPositions.IsInside(true, "http://localhost", false).ShouldBeFalse();
    [Fact] void should_let_a_mark_make_a_vendor_inside() => NetworkPositions.IsInside(false, "https://api.vendor.com", true).ShouldBeTrue();
    [Fact] void should_not_trust_a_private_endpoint_on_a_vendor_that_is_not_openai_compatible() => NetworkPositions.IsInside(false, "http://localhost", null).ShouldBeFalse();
    [Fact] void should_trust_a_private_endpoint_on_an_openai_compatible_provider() => NetworkPositions.IsInside(true, "http://localhost", null).ShouldBeTrue();
}
#endif
