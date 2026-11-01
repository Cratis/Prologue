// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Contracts;

/// <summary>
/// How the shadow system answered a mirrored command compared with how the legacy system did.
/// </summary>
public enum ParityOutcome
{
    /// <summary>
    /// Both systems decided the same way: both accepted the command or both refused it.
    /// </summary>
    Matched,

    /// <summary>
    /// The systems decided differently.
    /// </summary>
    Mismatched,

    /// <summary>
    /// The shadow system could not be reached, timed out or was unavailable. This says nothing about its decision.
    /// </summary>
    Unavailable,

    /// <summary>
    /// The command was not mirrored, for example because its body was too large to copy.
    /// </summary>
    Skipped
}
