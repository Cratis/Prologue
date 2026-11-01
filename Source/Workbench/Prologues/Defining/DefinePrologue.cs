// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Prologues.Defining;

/// <summary>
/// Defines a prologue: the work of capturing and interpreting one existing system.
/// </summary>
/// <param name="Id">The identity of the prologue; supplied by the caller so a retry reuses it.</param>
/// <param name="Name">What the prologue is called.</param>
[Command]
public record DefinePrologue(PrologueId Id, PrologueName Name) : ICanProvideEventSourceId
{
    /// <inheritdoc/>
    public EventSourceId GetEventSourceId() => Id;

    /// <summary>
    /// Handles the command, refusing a blank name or an identity that is already defined.
    /// </summary>
    /// <param name="existing">The prologue under this identity, when there is one.</param>
    /// <returns>The prologue defined, or a refusal.</returns>
    public Result<PrologueDefined, ValidationResult> Handle(PrologueDefinition? existing)
    {
        if (string.IsNullOrWhiteSpace(Name.Value))
        {
            return ValidationResult.Error("Give the prologue a name");
        }

        return existing is null ? new PrologueDefined(Name) : ValidationResult.Error("This prologue is already defined");
    }
}

/// <summary>
/// A prologue was defined.
/// </summary>
/// <param name="Name">What the prologue is called.</param>
[EventType]
public record PrologueDefined(PrologueName Name);
