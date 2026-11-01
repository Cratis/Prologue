// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Prologues;

/// <summary>
/// Where the Extractor sends what it captures.
/// </summary>
public enum CaptureOutput
{
    /// <summary>
    /// To the Receiver, which stores them for the live view.
    /// </summary>
    Api,

    /// <summary>
    /// To rolling JSON files.
    /// </summary>
    Json
}
