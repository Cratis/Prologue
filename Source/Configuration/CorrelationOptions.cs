// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Configuration;

/// <summary>
/// Represents the configuration for correlating observations from different sources into a single capture.
/// </summary>
public class CorrelationOptions
{
    /// <summary>
    /// Gets or sets the time window, in milliseconds, within which a command and the database transactions
    /// it produced are considered correlated.
    /// </summary>
    public int WindowMilliseconds { get; set; } = 2000;

    /// <summary>
    /// Gets the correlation window as a <see cref="TimeSpan"/>.
    /// </summary>
    public TimeSpan Window => TimeSpan.FromMilliseconds(WindowMilliseconds);
}
