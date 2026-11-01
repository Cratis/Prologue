// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Extractor.Shadowing;

/// <summary>
/// Compares how the legacy system and the shadow system decided about the same command.
/// </summary>
public static class ShadowComparison
{
    /// <summary>
    /// Compares the two answers with the classification rule the Workbench uses for captured decisions (<see cref="HttpOutcomes"/>):
    /// 2xx and 3xx accept a command, 4xx and 5xx refuse it, and the systems match when they decided the same way, whatever the exact status.
    /// A shadow system that did not answer, or answered with a gateway error (502, 503, 504) or with nothing that decides, is
    /// unavailable: that says nothing about its decisions and is never counted as a mismatch.
    /// </summary>
    /// <param name="legacyStatus">The status the legacy system answered with.</param>
    /// <param name="shadowStatus">The status the shadow system answered with, or null when it did not answer.</param>
    /// <returns>The outcome.</returns>
    public static ParityOutcome Compare(int legacyStatus, int? shadowStatus)
    {
        if (shadowStatus is not { } shadow || IsGatewayError(shadow) || HttpOutcomes.Classify(shadow) == HttpOutcome.NotDecided)
        {
            return ParityOutcome.Unavailable;
        }

        return HttpOutcomes.Classify(legacyStatus) == HttpOutcomes.Classify(shadow) ? ParityOutcome.Matched : ParityOutcome.Mismatched;
    }

    /// <summary>
    /// Gets whether the legacy answer decides anything that can be compared. An answer that is neither a success nor a refusal is not mirrored.
    /// </summary>
    /// <param name="legacyStatus">The status the legacy system answered with.</param>
    /// <returns>True when it is a decision.</returns>
    public static bool IsDecision(int legacyStatus) => HttpOutcomes.Classify(legacyStatus) != HttpOutcome.NotDecided;

    static bool IsGatewayError(int status) => status is 502 or 503 or 504;
}
