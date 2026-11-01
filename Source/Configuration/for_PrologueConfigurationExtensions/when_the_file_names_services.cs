// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Microsoft.Extensions.Configuration;

namespace Cratis.Prologue.Configuration.for_PrologueConfigurationExtensions;

/// <summary>
/// Named services sit next to the default service's top-level sources in cratis-prologue.json, and a deployed tool
/// must be able to add to them from the environment just like any other setting.
/// </summary>
public class when_the_file_names_services : Specification, IDisposable
{
    const string ConnectionStringVariable = "Prologue__Services__billing__Postgres__0__ConnectionString";

    string _directory;
    PrologueOptions _result;

    public void Dispose()
    {
        Environment.SetEnvironmentVariable(ConnectionStringVariable, null);

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

        File.WriteAllText(
            Path.Combine(_directory, PrologueConfigurationFile.FileName),
            """
            {
                "prologue": {
                    "sqlServer": [ { "name": "legacy", "connectionString": "Server=legacy" } ],
                    "services": {
                        "orders": {
                            "sqlServer": [ { "name": "orders", "connectionString": "Server=orders" } ],
                            "clusters": [ "orders-api" ],
                            "telemetryServiceNames": [ "Orders.Api" ]
                        },
                        "billing": {
                            "postgres": [ { "name": "billing" } ]
                        }
                    }
                }
            }
            """);

        Environment.SetEnvironmentVariable(ConnectionStringVariable, "Host=billing");
    }

    void Because() =>
        _result = new ConfigurationBuilder()
            .AddPrologueConfiguration(_directory)
            .Build()
            .GetSection(PrologueOptions.SectionName)
            .Get<PrologueOptions>() ?? new PrologueOptions();

    [Fact] void should_keep_the_top_level_sources_for_the_default_service() => _result.SqlServer[0].Name.ShouldEqual("legacy");
    [Fact] void should_bind_every_named_service() => _result.Services.Count.ShouldEqual(2);
    [Fact] void should_bind_a_named_services_databases() => _result.Services["orders"].SqlServer[0].ConnectionString.ShouldEqual("Server=orders");
    [Fact] void should_bind_a_named_services_clusters() => _result.Services["orders"].Clusters[0].ShouldEqual("orders-api");
    [Fact] void should_bind_a_named_services_telemetry_service_names() => _result.Services["orders"].TelemetryServiceNames[0].ShouldEqual("Orders.Api");
    [Fact] void should_take_a_named_services_setting_from_the_environment() => _result.Services["billing"].Postgres[0].ConnectionString.ShouldEqual("Host=billing");
}
#endif
