// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Configuration;
using Cratis.Prologue.Workbench.Systems;

namespace Cratis.Prologue.Workbench.Prologues.Configuration;

/// <summary>
/// Generates the <c language="json">cratis-prologue.json</c> that matches a prologue's definition. Secrets appear by name only.
/// </summary>
public static class PrologueConfigurationGenerator
{
    /// <summary>
    /// Builds the configuration for a prologue: one named service per system.
    /// </summary>
    /// <param name="prologue">The prologue.</param>
    /// <returns>The configuration the Extractor binds.</returns>
    public static PrologueConfiguration ToConfiguration(PrologueDefinition prologue)
    {
        var options = new PrologueOptions
        {
            PrologueId = prologue.Id.Value,
            Output = new OutputOptions { Kind = prologue.Output == CaptureOutput.Json ? OutputKind.Json : OutputKind.Api },
            Correlation = new CorrelationOptions { WindowMilliseconds = prologue.Correlation.Value }
        };

        foreach (var system in prologue.Systems)
        {
            options.Services[system.Name.Value] = ToService(system);
        }

        List<string> services = [.. prologue.Systems.SelectMany(system => system.TelemetryServices).Select(service => service.Value).Distinct()];
        options.OpenTelemetry = new OpenTelemetryOptions
        {
            Enabled = services.Count > 0,
            ServiceNames = services,
            AttributeKeys = [.. prologue.Systems.SelectMany(system => system.AttributeKeys).Select(key => key.Value).Distinct()]
        };

        return new PrologueConfiguration { Prologue = options };
    }

    /// <summary>
    /// Builds the text of <c language="json">cratis-prologue.json</c> for a prologue.
    /// </summary>
    /// <param name="prologue">The prologue.</param>
    /// <returns>The JSON.</returns>
    public static string ToJson(PrologueDefinition prologue) => PrologueConfigurationFile.Write(ToConfiguration(prologue));

    static ServiceOptions ToService(PrologueSystem system)
    {
        var service = new ServiceOptions
        {
            Clusters = [.. system.HttpClusters.Select(cluster => cluster.Value)],
            TelemetryServiceNames = [.. system.TelemetryServices.Select(name => name.Value)]
        };

        var databases = system.Databases.ToList();
        for (var index = 0; index < databases.Count; index++)
        {
            var database = databases[index];
            var name = databases.Count == 1 ? system.Name.Value : $"{system.Name.Value}-{index + 1}";
            var connectionString = $"${{secret:{database.ConnectionString.Value}}}";
            var tables = database.Tables.Select(table => table.Value).ToArray();

            if (database.Engine == DatabaseEngine.SqlServer)
            {
                service.SqlServer.Add(new SqlServerOptions { Name = name, ConnectionString = connectionString, Tables = tables });
            }
            else
            {
                service.Postgres.Add(new PostgresOptions { Name = name, ConnectionString = connectionString });
            }
        }

        return service;
    }
}
