// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Captures;

namespace Cratis.Prologue.Workbench.Rebuild.Drift;

/// <summary>
/// A command shape the system was seen to have before the model was accepted, with the columns it wrote.
/// </summary>
/// <param name="Shape">The command shape.</param>
/// <param name="Writes">The columns it was seen writing.</param>
public record KnownShape(CommandShape Shape, IEnumerable<WrittenColumn> Writes);
