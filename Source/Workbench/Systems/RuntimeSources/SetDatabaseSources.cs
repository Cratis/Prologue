// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Common;
using Cratis.Prologue.Workbench.Prologues;
using Cratis.Prologue.Workbench.Repositories.Connecting;

namespace Cratis.Prologue.Workbench.Systems.RuntimeSources;

/// <summary>
/// Sets the databases whose transaction logs are read for a system. A database is named by the secret holding its
/// connection string; the connection string itself is never part of the command.
/// </summary>
/// <param name="Prologue">The prologue.</param>
/// <param name="Id">The system.</param>
/// <param name="Sources">The databases; empty stops database capture for the system.</param>
[Command]
[ReadProtection(ReadProtectionMode.On)]
public record SetDatabaseSources(PrologueId Prologue, SystemId Id, IEnumerable<DatabaseSource> Sources)
{
    /// <summary>
    /// Handles the command, refusing an unknown system, a database with no secret name and sources the system already has.
    /// </summary>
    /// <param name="prologue">The prologue.</param>
    /// <returns>The databases set, or a refusal.</returns>
    public Result<SystemDatabaseSourcesSet, ValidationResult> Handle(PrologueDefinition? prologue)
    {
        if (SystemRules.RefuseUnknown(prologue, Id) is { } unknown)
        {
            return unknown;
        }

        if (Sources.Any(source => string.IsNullOrWhiteSpace(source.ConnectionString.Value)))
        {
            return ValidationResult.Error("Name the secret that holds the connection string");
        }

        if (Sources.Select(source => ConnectionRules.RefuseSecretName(new SecretName(source.ConnectionString.Value.Trim()))).FirstOrDefault(refusal => refusal is not null) is { } invalid)
        {
            return invalid;
        }

        var sources = Sources
            .Select(source => new DatabaseSource(
                source.Engine,
                new SecretName(source.ConnectionString.Value.Trim()),
                [.. SystemRules.Clean(source.Tables.Select(table => table.Value)).Select(table => new TableName(table))]))
            .ToArray();

        return SameSources(SystemRules.Find(prologue, Id)!.Databases, sources)
            ? ValidationResult.Error("The system already has these databases")
            : new SystemDatabaseSourcesSet(Id, sources);
    }

    static bool SameSources(IEnumerable<DatabaseSource> current, IEnumerable<DatabaseSource> next) =>
        current.Count() == next.Count() &&
        current.Zip(next).All(pair =>
            pair.First.Engine == pair.Second.Engine &&
            pair.First.ConnectionString == pair.Second.ConnectionString &&
            pair.First.Tables.SequenceEqual(pair.Second.Tables));
}

/// <summary>
/// The databases read for a system were set.
/// </summary>
/// <param name="Id">The system.</param>
/// <param name="Sources">The databases, each by the name of its connection string secret.</param>
[EventType]
public record SystemDatabaseSourcesSet(SystemId Id, IEnumerable<DatabaseSource> Sources);
