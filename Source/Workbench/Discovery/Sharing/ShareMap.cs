// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Arc.Chronicle.ReadModels;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Discovery.Sharing;

/// <summary>
/// Shares the map of a prologue through a read-only link. The link shows the application map, the decision points and the
/// dependencies, without the controls that change anything.
/// </summary>
/// <param name="Prologue">The prologue.</param>
[Command]
[ReadProtection(ReadProtectionMode.On)]
public record ShareMap(PrologueId Prologue) : ICanProvideEventSourceId
{
    /// <inheritdoc/>
    public EventSourceId GetEventSourceId() => Prologue;

    /// <summary>
    /// Handles the command, refusing an undefined prologue and a map that is shared already.
    /// </summary>
    /// <param name="prologue">The prologue.</param>
    /// <param name="shared">Whether and how the map is shared.</param>
    /// <returns>The share, or a refusal.</returns>
    public Result<MapShared, ValidationResult> Handle(PrologueDefinition? prologue, [Unprotected("Two shares at once would leave the later link in force; the guard only avoids a second link")] SharedMap? shared)
    {
        if (prologue is null)
        {
            return ValidationResult.Error("The prologue is not defined");
        }

        return shared is { IsShared: true }
            ? ValidationResult.Error("The map is shared already; stop sharing it before sharing it again")
            : new MapShared(ShareToken.New());
    }
}

/// <summary>
/// The map of a prologue was shared through a read-only link.
/// </summary>
/// <param name="Token">The token in the link.</param>
[EventType]
public record MapShared(ShareToken Token);
