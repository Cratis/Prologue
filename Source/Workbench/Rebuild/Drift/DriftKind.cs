// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Rebuild.Drift;

/// <summary>
/// How the legacy system changed after the model was accepted.
/// </summary>
public enum DriftKind
{
    /// <summary>
    /// A command shape the model did not cover was seen.
    /// </summary>
    NewShape,

    /// <summary>
    /// A command shape the model covers started writing a column it did not write before.
    /// </summary>
    NewColumn
}
