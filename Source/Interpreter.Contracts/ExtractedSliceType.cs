// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Interpreter.Contracts;

/// <summary>
/// Represents the type of a slice the interpreter extracted from a Prologue's captures. Mirrors the authoring
/// model's slice types (State Change, State View, Automation, Translation).
/// </summary>
public enum ExtractedSliceType
{
    /// <summary>
    /// A slice that accepts a command and changes state by appending events.
    /// </summary>
    StateChange,

    /// <summary>
    /// A slice that projects events into a queryable read model.
    /// </summary>
    StateView,

    /// <summary>
    /// A slice that reacts to events and performs side effects against an external system.
    /// </summary>
    Automation,

    /// <summary>
    /// A slice that reacts to events and appends follow-up events to another stream.
    /// </summary>
    Translation
}
