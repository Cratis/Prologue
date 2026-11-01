// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Common;
using Microsoft.Extensions.Configuration;

namespace Cratis.Prologue.Workbench.Repositories.Cloning.for_ConfigurationSecretResolver;

public class when_resolving_a_secret : Specification, IDisposable
{
    const string Variable = "ADO_PROLOGUE_READ_SPEC";

    string _fromConfiguration;
    string _fromEnvironment;
    string _missing;

    public void Dispose()
    {
        Environment.SetEnvironmentVariable(Variable, null);
        GC.SuppressFinalize(this);
    }

    void Because()
    {
        Environment.SetEnvironmentVariable(Variable, "from-the-environment");
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Prologue:Secrets:github/prologue-read"] = "from-configuration" })
            .Build();
        var resolver = new ConfigurationSecretResolver(configuration);

        _fromConfiguration = resolver.Resolve(new SecretName("github/prologue-read"));
        _fromEnvironment = resolver.Resolve(new SecretName("ado/prologue-read-spec"));
        _missing = resolver.Resolve(new SecretName("not/configured"));
    }

    [Fact] void should_resolve_from_configuration_by_name() => _fromConfiguration.ShouldEqual("from-configuration");
    [Fact] void should_resolve_from_the_environment_by_the_normalized_name() => _fromEnvironment.ShouldEqual("from-the-environment");
    [Fact] void should_resolve_nothing_for_an_unknown_name() => _missing.ShouldBeNull();
}
#endif
