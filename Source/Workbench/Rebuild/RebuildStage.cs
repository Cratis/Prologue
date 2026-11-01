// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Rebuild;

/// <summary>
/// How far a slice has come on its way from the legacy system to the new one. A slice moves one stage at a time.
/// </summary>
public enum RebuildStage
{
    /// <summary>
    /// The slice is known from what was observed; nothing has been modeled.
    /// </summary>
    Observed,

    /// <summary>
    /// The slice is part of the event model.
    /// </summary>
    Modeled,

    /// <summary>
    /// The new slice exists.
    /// </summary>
    Built,

    /// <summary>
    /// The Extractor mirrors the slice's commands to the new slice and compares its decisions with the legacy system's.
    /// </summary>
    Shadowing,

    /// <summary>
    /// The route switched: the new slice serves it.
    /// </summary>
    CutOver
}
