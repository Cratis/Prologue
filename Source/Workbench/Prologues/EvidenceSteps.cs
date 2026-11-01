// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Prologues;

/// <summary>
/// Which evidence steps a prologue gathers.
/// </summary>
/// <param name="Discover">Whether the systems' source code is analyzed.</param>
/// <param name="Capture">Whether the running systems are watched.</param>
/// <param name="Rebuild">Whether the new system is tracked against the captured decisions.</param>
public record EvidenceSteps(bool Discover, bool Capture, bool Rebuild)
{
    /// <summary>
    /// Every step on, which is how a prologue starts.
    /// </summary>
    public static readonly EvidenceSteps All = new(true, true, true);

    /// <summary>
    /// Gets whether a step is on.
    /// </summary>
    /// <param name="step">The step.</param>
    /// <returns>True when it is on.</returns>
    public bool IsOn(EvidenceStep step) => step switch
    {
        EvidenceStep.Discover => Discover,
        EvidenceStep.Capture => Capture,
        _ => Rebuild
    };

    /// <summary>
    /// Gets these steps with one step turned on or off.
    /// </summary>
    /// <param name="step">The step.</param>
    /// <param name="on">Whether it is on.</param>
    /// <returns>The new steps.</returns>
    public EvidenceSteps With(EvidenceStep step, bool on) => step switch
    {
        EvidenceStep.Discover => this with { Discover = on },
        EvidenceStep.Capture => this with { Capture = on },
        _ => this with { Rebuild = on }
    };
}
