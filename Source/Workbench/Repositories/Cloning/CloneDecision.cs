// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Repositories.Cloning;

/// <summary>
/// Decides what a clone is worth recording. Looking again is not a fact: a clone that ends where the last one did records nothing.
/// </summary>
public static class CloneDecision
{
    /// <summary>
    /// Decides the facts to record for a clone result.
    /// </summary>
    /// <param name="result">What the clone ended in.</param>
    /// <param name="previous">What was recorded before.</param>
    /// <returns>The event to record; none when nothing changed.</returns>
    public static IEnumerable<object> Decide(CloneResult result, SystemClone? previous)
    {
        if (result.Failure is { } failure)
        {
            return previous is { Status: CloneStatus.Failed } && previous.Reason == failure ? [] : [new SystemRepositoryCloneFailed(failure)];
        }

        var cloned = new SystemRepositoryCloned(result.Commit!, result.Files!);
        return previous is { Status: CloneStatus.Cloned } && previous.Commit == cloned.Commit && previous.Files == cloned.Files ? [] : [cloned];
    }
}
