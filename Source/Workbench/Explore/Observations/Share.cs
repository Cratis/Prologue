// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Explore.Observations;

/// <summary>
/// Turns counts into the whole percentages Explore shows.
/// </summary>
public static class Share
{
    /// <summary>
    /// Gets a part of a whole as a whole percentage. Something that happened at all never reads as 0%.
    /// </summary>
    /// <param name="part">The part.</param>
    /// <param name="whole">The whole.</param>
    /// <returns>The percentage, 0 only when the part is 0.</returns>
    public static int Percent(int part, int whole)
    {
        if (part <= 0 || whole <= 0)
        {
            return 0;
        }

        return Math.Clamp((int)Math.Round(100.0 * part / whole, MidpointRounding.AwayFromZero), 1, 100);
    }
}
