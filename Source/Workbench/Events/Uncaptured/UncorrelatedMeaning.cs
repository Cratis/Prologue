// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Events.Uncaptured;

/// <summary>
/// What writes with no command turned out to be.
/// </summary>
public enum UncorrelatedMeaning
{
    /// <summary>
    /// A job that runs on its own.
    /// </summary>
    Job,

    /// <summary>
    /// People correcting data by hand.
    /// </summary>
    Correction,

    /// <summary>
    /// Not part of the model.
    /// </summary>
    Ignored
}
