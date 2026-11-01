// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Discover.Runtime;

/// <summary>
/// Whether a capability found in the code was seen at runtime.
/// </summary>
public enum RuntimeStatus
{
    /// <summary>
    /// A captured command shape belongs to it.
    /// </summary>
    Seen,

    /// <summary>
    /// No captured command belongs to it.
    /// </summary>
    CodeOnly
}
