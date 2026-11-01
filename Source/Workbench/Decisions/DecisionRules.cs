// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Captures;

namespace Cratis.Prologue.Workbench.Decisions;

/// <summary>
/// Classifies what the observed system decided about a command.
/// </summary>
public static class DecisionRules
{
    const int FirstAccepted = 200;
    const int FirstRefused = 400;
    const int AfterLastRefused = 600;

    /// <summary>
    /// Classifies a command by the status it was answered with, whatever was written: 2xx and 3xx accept it, with or
    /// without database writes, and 4xx and 5xx refuse it. A capture with no command, or a status outside 200-599, is not a decision.
    /// What was written explains a decision but never changes it.
    /// </summary>
    /// <param name="http">The command of the capture.</param>
    /// <returns>The outcome.</returns>
    public static DecisionOutcome Classify(RecordedCommand http) => http.Shape.IsNone ? DecisionOutcome.NotDecided : http.Status.Value switch
    {
        >= FirstAccepted and < FirstRefused => DecisionOutcome.Accepted,
        >= FirstRefused and < AfterLastRefused => DecisionOutcome.Refused,
        _ => DecisionOutcome.NotDecided
    };
}
