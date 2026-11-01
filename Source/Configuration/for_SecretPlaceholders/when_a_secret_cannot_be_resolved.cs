// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Microsoft.Extensions.Configuration;

namespace Cratis.Prologue.Configuration.for_SecretPlaceholders;

public class when_a_secret_cannot_be_resolved : Specification, IDisposable
{
    string _directory;
    Exception _result;

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }

        GC.SuppressFinalize(this);
    }

    void Establish()
    {
        _directory = Path.Combine(Path.GetTempPath(), $"prologue-secrets-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_directory);
        File.WriteAllText(
            Path.Combine(_directory, PrologueConfigurationFile.FileName),
            """{ "prologue": { "services": { "a": { "postgres": [ { "name": "a", "connectionString": "${secret:no-such/secret-for-spec}" } ] } } } }""");
    }

    void Because() => _result = Catch.Exception(() => new ConfigurationBuilder().AddPrologueConfiguration(_directory).Build());

    [Fact] void should_fail() => _result.ShouldBeOfExactType<SecretNotConfigured>();
    [Fact] void should_name_the_secret() => _result.Message.ShouldContain("no-such/secret-for-spec");
    [Fact] void should_name_where_to_set_it() => _result.Message.ShouldContain("NO_SUCH_SECRET_FOR_SPEC");
}
#endif
