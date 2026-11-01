// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Configuration;
using Cratis.Prologue.Workbench.Prologues.Configuration;
using Cratis.Prologue.Workbench.Prologues.given;
using Cratis.Prologue.Workbench.Prologues.Output;

namespace Cratis.Prologue.Workbench.Prologues.for_PrologueConfigurationGenerator;

public class when_pointing_the_extractor_at_the_receiver : a_prologue
{
    PrologueConfiguration _withReceiver;
    PrologueConfiguration _withPath;
    PrologueConfiguration _withoutReceiver;
    PrologueConfiguration _withGarbage;
    PrologueConfiguration _json;
    PrologueConfiguration _jsonOutput;

    void Because()
    {
        _withReceiver = PrologueConfigurationGenerator.ToConfiguration(_prologue, "https://prologue.example.com");
        _withPath = PrologueConfigurationGenerator.ToConfiguration(_prologue, "http://localhost:5100/systems?x=1");
        _withoutReceiver = PrologueConfigurationGenerator.ToConfiguration(_prologue);
        _withGarbage = PrologueConfigurationGenerator.ToConfiguration(_prologue, "javascript:alert(1)");
        _jsonOutput = PrologueConfigurationGenerator.ToConfiguration(PrologueDefinition.Reduce(new OutputChosen(CaptureOutput.Json), _prologue)!, "http://receiver:5100");
        _json = PrologueConfigurationFile.Read(PrologueConfigurationGenerator.ToJson(_prologue, "http://receiver:5100/"));
    }

    [Fact] void should_use_the_receiver_address_as_the_api_endpoint() => _withReceiver.Prologue.Output.Api.Endpoint.ShouldEqual("https://prologue.example.com");
    [Fact] void should_keep_only_the_scheme_host_and_port() => _withPath.Prologue.Output.Api.Endpoint.ShouldEqual("http://localhost:5100");
    [Fact] void should_keep_the_default_endpoint_without_a_receiver() => _withoutReceiver.Prologue.Output.Api.Endpoint.ShouldEqual(new ApiOptions().Endpoint);
    [Fact] void should_ignore_an_address_that_is_not_http() => _withGarbage.Prologue.Output.Api.Endpoint.ShouldEqual(new ApiOptions().Endpoint);
    [Fact] void should_write_the_endpoint_into_the_file() => _json.Prologue.Output.Api.Endpoint.ShouldEqual("http://receiver:5100");
    [Fact] void should_keep_the_json_output_kind() => _jsonOutput.Prologue.Output.Kind.ShouldEqual(OutputKind.Json);
    [Fact] void should_keep_the_api_kind() => _withReceiver.Prologue.Output.Kind.ShouldEqual(OutputKind.Api);
}
#endif
