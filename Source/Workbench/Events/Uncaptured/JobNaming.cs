// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Interpretation;

namespace Cratis.Prologue.Workbench.Events.Uncaptured;

/// <summary>
/// Names the event a job's writes record, from what the job is said to do. The name is provisional: the Interpreter agent or a person refines it.
/// </summary>
public static class JobNaming
{
    static readonly (string Stem, string Past)[] _verbs =
    [
        ("reconcil", "Reconciled"),
        ("expir", "Expired"),
        ("archiv", "Archived"),
        ("purg", "Purged"),
        ("prun", "Purged"),
        ("clean", "Purged"),
        ("sync", "Synchronized"),
        ("recalc", "Recalculated"),
        ("refresh", "Refreshed"),
        ("rebuild", "Rebuilt"),
        ("rotat", "Rotated"),
        ("overdue", "MarkedOverdue"),
        ("settl", "Settled")
    ];

    /// <summary>
    /// Names the event.
    /// </summary>
    /// <param name="entity">The entity the table holds, singular and in PascalCase.</param>
    /// <param name="job">What the job is called and does; empty when no job is known.</param>
    /// <param name="operation">The operation most of the writes are.</param>
    /// <returns>The event name: the entity and what the job does to it, or the operation when the job says nothing recognizable.</returns>
    public static string EventName(string entity, string job, ChangeOperation operation)
    {
        var text = job.ToLowerInvariant();
        foreach (var (stem, past) in _verbs)
        {
            if (text.Contains(stem, StringComparison.Ordinal))
            {
                return $"{entity}{past}";
            }
        }

        return Naming.EventName(entity, operation);
    }
}
