// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Configuration;
using Cratis.Prologue.Workbench.Common;
using Cratis.Prologue.Workbench.Prologues.Configuration;
using Cratis.Prologue.Workbench.Prologues.given;
using Cratis.Prologue.Workbench.Systems;
using Cratis.Prologue.Workbench.Systems.RuntimeSources;
using Microsoft.Extensions.Configuration;

namespace Cratis.Prologue.Workbench.Prologues.for_PrologueConfigurationGenerator;

public class when_generating_for_a_prologue_with_systems : a_prologue, IDisposable
{
    string _directory;
    string _json;
    PrologueOptions _bound;

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }

        GC.SuppressFinalize(this);
    }

    void Because()
    {
        var withClusters = PrologueDefinition.Reduce(new SystemHttpClustersSet(_lending, [new ClusterName("lending-api")]), _prologue);
        var withDatabase = PrologueDefinition.Reduce(
            new SystemDatabaseSourcesSet(_lending, [new DatabaseSource(DatabaseEngine.SqlServer, new SecretName("library-db/connection"), [new TableName("Loans")])]),
            withClusters);
        var withTelemetry = PrologueDefinition.Reduce(new SystemTelemetrySet(_lending, [new TelemetryServiceName("Lending.Api")], [new AttributeKey("http.route")]), withDatabase);
        _json = PrologueConfigurationGenerator.ToJson(withTelemetry);

        // Bind it the way the Extractor and the Receiver do: from cratis-prologue.json through configuration.
        _directory = Path.Combine(Path.GetTempPath(), $"prologue-generated-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_directory);
        File.WriteAllText(Path.Combine(_directory, PrologueConfigurationFile.FileName), _json);
        _bound = new ConfigurationBuilder().AddPrologueConfiguration(_directory).Build().GetSection(PrologueOptions.SectionName).Get<PrologueOptions>();
    }

    [Fact] void should_list_every_system_as_a_service() => _bound.Services.Keys.ShouldContainOnly("Lending", "Search");
    [Fact] void should_bind_the_prologue_identity() => _bound.PrologueId.ShouldEqual(_id.Value);
    [Fact] void should_bind_the_clusters_of_a_service() => _bound.Services["Lending"].Clusters.Single().ShouldEqual("lending-api");
    [Fact] void should_bind_the_telemetry_service_names_of_a_service() => _bound.Services["Lending"].TelemetryServiceNames.Single().ShouldEqual("Lending.Api");
    [Fact] void should_allow_only_the_listed_tables() => _bound.Services["Lending"].SqlServer.Single().Tables.Single().ShouldEqual("Loans");
    [Fact] void should_allowlist_the_attribute_keys() => _bound.OpenTelemetry.AttributeKeys.Single().ShouldEqual("http.route");
    [Fact] void should_name_the_secret_for_the_connection_string() => _bound.Services["Lending"].SqlServer.Single().ConnectionString.ShouldEqual("<from secret library-db/connection>");
    [Fact] void should_leave_a_system_without_sources_empty() => _bound.Services["Search"].SqlServer.ShouldBeEmpty();
}
#endif
