// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Events.Interpreting;

/// <summary>
/// How strongly the evidence supports a suggested event.
/// </summary>
public static class SuggestionConfidence
{
    /// <summary>
    /// Works out the confidence: it grows with the number of captures behind the event, drops when the event shows up in only some of the captures
    /// of the command that leads to it, and gains a little when the code confirms the command.
    /// </summary>
    /// <param name="captures">How many captures show the event.</param>
    /// <param name="commandCaptures">How many captures there are of the commands that lead to it.</param>
    /// <param name="codeConfirms">Whether Discover found the command in the code.</param>
    /// <returns>The confidence, between 0 and 0.99.</returns>
    public static Confidence For(int captures, int commandCaptures, bool codeConfirms)
    {
        if (captures <= 0)
        {
            return 0.3;
        }

        var strength = 1 - (1 / (captures + 1.2));
        var share = commandCaptures <= 0 ? 1 : Math.Min(1.0, (double)captures / commandCaptures);
        var confidence = (strength * (0.6 + (0.4 * share))) + (codeConfirms ? 0.04 : 0);
        return Math.Round(Math.Min(0.99, confidence), 2, MidpointRounding.AwayFromZero);
    }
}
