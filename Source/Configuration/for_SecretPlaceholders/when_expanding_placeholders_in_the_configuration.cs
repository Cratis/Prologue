// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Microsoft.Extensions.Configuration;

namespace Cratis.Prologue.Configuration.for_SecretPlaceholders;

public class when_expanding_placeholders_in_the_configuration : Specification, IDisposable
{
    const string EnvironmentVariable = "PROLOGUE_SECRET_LIBRARY_DB_CONNECTION";

    string _directory;
    PrologueOptions _result;

    public void Dispose()
    {
        Environment.SetEnvironmentVariable(EnvironmentVariable, null);
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
            """
            {
                "prologue": {
                    "secrets": { "billing/db": "from-configuration" },
                    "services": {
                        "lending": {
                            "sqlServer": [ { "name": "lending", "connectionString": "${secret:library-db/connection}" } ],
                            "postgres": [ { "name": "billing", "connectionString": "Host=db;Password=${secret:billing/db}" } ]
                        }
                    }
                }
            }
            """);
        Environment.SetEnvironmentVariable(EnvironmentVariable, "from-environment");
    }

    void Because() =>
        _result = new ConfigurationBuilder()
            .AddPrologueConfiguration(_directory)
            .Build()
            .GetSection(PrologueOptions.SectionName)
            .Get<PrologueOptions>();

    [Fact] void should_expand_a_secret_from_the_environment() => _result.Services["lending"].SqlServer.Single().ConnectionString.ShouldEqual("from-environment");
    [Fact] void should_expand_a_secret_from_configuration_inside_a_longer_value() => _result.Services["lending"].Postgres.Single().ConnectionString.ShouldEqual("Host=db;Password=from-configuration");
}
#endif
