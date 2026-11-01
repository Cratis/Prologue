// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Systems.RuntimeSources;

/// <summary>
/// Sets the HTTP clusters Capture records commands from for a system.
/// </summary>
/// <param name="Prologue">The prologue.</param>
/// <param name="Id">The system.</param>
/// <param name="Clusters">The clusters; empty stops HTTP capture for the system.</param>
[Command]
[ReadProtection(ReadProtectionMode.On)]
public record SetHttpClusters(PrologueId Prologue, SystemId Id, IEnumerable<ClusterName> Clusters)
{
    /// <summary>
    /// Handles the command, refusing an unknown system and clusters the system already has.
    /// </summary>
    /// <param name="prologue">The prologue.</param>
    /// <returns>The clusters set, or a refusal.</returns>
    public Result<SystemHttpClustersSet, ValidationResult> Handle(PrologueDefinition? prologue)
    {
        if (SystemRules.RefuseUnknown(prologue, Id) is { } unknown)
        {
            return unknown;
        }

        var clusters = SystemRules.Clean(Clusters.Select(cluster => cluster.Value)).Select(cluster => new ClusterName(cluster)).ToArray();
        return SystemRules.Find(prologue, Id)!.HttpClusters.SequenceEqual(clusters)
            ? ValidationResult.Error("The system already has these clusters")
            : new SystemHttpClustersSet(Id, clusters);
    }
}

/// <summary>
/// The HTTP clusters recorded for a system were set.
/// </summary>
/// <param name="Id">The system.</param>
/// <param name="Clusters">The clusters.</param>
[EventType]
public record SystemHttpClustersSet(SystemId Id, IEnumerable<ClusterName> Clusters);
