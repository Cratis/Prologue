// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Microsoft.Extensions.Configuration;

namespace Cratis.Prologue.Configuration.for_PrologueConfigurationExtensions;

/// <summary>
/// The composition of a deployed Prologue tool configures it through the environment while the checked-in
/// cratis-prologue.json supplies the baseline. If the file were to win, every environment override would be
/// silently ignored — so the precedence is pinned here rather than assumed.
/// </summary>
public class when_the_environment_overrides_the_file : Specification, IDisposable
{
    const string OutputKindVariable = "Prologue__Output__Kind";
    const string EndpointVariable = "Prologue__Output__Api__Endpoint";

    string _directory;
    PrologueOptions _result;

    public void Dispose()
    {
        Environment.SetEnvironmentVariable(OutputKindVariable, null);
        Environment.SetEnvironmentVariable(EndpointVariable, null);

        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }

        GC.SuppressFinalize(this);
    }

    void Establish()
    {
        _directory = Path.Combine(Path.GetTempPath(), $"prologue-config-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_directory);

        // The file says write capture files to disk, and names no receiver.
        File.WriteAllText(
            Path.Combine(_directory, PrologueConfigurationFile.FileName),
            """
            {
                "prologue": {
                    "output": {
                        "kind": "Json",
                        "json": { "directory": "./captures" }
                    },
                    "correlation": { "windowMilliseconds": 1234 }
                }
            }
            """);

        // The host says post them to a receiver instead.
        Environment.SetEnvironmentVariable(OutputKindVariable, "Api");
        Environment.SetEnvironmentVariable(EndpointVariable, "http://receiver:5005");
    }

    void Because() =>
        _result = new ConfigurationBuilder()
            .AddPrologueConfiguration(_directory)
            .Build()
            .GetSection(PrologueOptions.SectionName)
            .Get<PrologueOptions>() ?? new PrologueOptions();

    [Fact] void should_take_the_output_kind_from_the_environment() => _result.Output.Kind.ShouldEqual(OutputKind.Api);
    [Fact] void should_take_the_endpoint_from_the_environment() => _result.Output.Api.Endpoint.ShouldEqual("http://receiver:5005");
    [Fact] void should_keep_the_file_value_the_environment_does_not_override() => _result.Correlation.WindowMilliseconds.ShouldEqual(1234);
}
#endif
