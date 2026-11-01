// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Events.Suggestions;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Events.Boundaries;

/// <summary>
/// Chooses how a consistency boundary is held. The choice is part of the accepted model, as a rule or constraint. Choosing what is chosen records nothing.
/// </summary>
/// <param name="Prologue">The prologue.</param>
/// <param name="Boundary">The boundary.</param>
/// <param name="Option">The key of the option.</param>
[Command]
[ReadProtection(ReadProtectionMode.On)]
public record ChooseConsistencyBoundary(PrologueId Prologue, BoundaryKey Boundary, string Option)
{
    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <param name="review">The review, read under the command's consistency boundary.</param>
    /// <returns>The event, nothing, or a refusal.</returns>
    public Result<IEnumerable<ConsistencyBoundaryChosen>, ValidationResult> Handle(EventReview? review)
    {
        var boundary = review?.Boundaries.FirstOrDefault(candidate => candidate.Key == Boundary);
        if (boundary is null)
        {
            return ValidationResult.Error("The evidence no longer raises this boundary. Interpret again");
        }

        var option = boundary.Options.FirstOrDefault(candidate => candidate.Key == Option);
        if (option is null)
        {
            return ValidationResult.Error("This boundary has no such option");
        }

        var chosen = review!.Choices.FirstOrDefault(choice => choice.Key == Boundary);
        IEnumerable<ConsistencyBoundaryChosen> events = chosen?.Option == option.Key ? [] : [new ConsistencyBoundaryChosen(Boundary, option.Key, option.Rule)];
        return Result<IEnumerable<ConsistencyBoundaryChosen>, ValidationResult>.Success(events);
    }
}

/// <summary>
/// A consistency boundary was decided.
/// </summary>
/// <param name="Key">The boundary.</param>
/// <param name="Option">The key of the option chosen.</param>
/// <param name="Rule">The rule or constraint the choice adds to the accepted model.</param>
[EventType]
public record ConsistencyBoundaryChosen(BoundaryKey Key, string Option, string Rule);
