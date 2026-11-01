// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Repositories.Cloning;

/// <summary>
/// What a clone command works out before it clones.
/// </summary>
/// <param name="Request">What to clone.</param>
/// <param name="Previous">What was last recorded for the system, when anything was.</param>
public record CloneTarget(CloneRequest Request, SystemClone? Previous);
