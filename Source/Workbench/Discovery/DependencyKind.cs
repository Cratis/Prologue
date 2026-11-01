// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Discovery;

/// <summary>
/// How one part of a system depends on another.
/// </summary>
public enum DependencyKind
{
    /// <summary>
    /// A call to an HTTP endpoint of another service.
    /// </summary>
    Http,

    /// <summary>
    /// Reads or writes a database.
    /// </summary>
    Database,

    /// <summary>
    /// Runs on a schedule or as a background job.
    /// </summary>
    Job,

    /// <summary>
    /// Sends or receives messages through a queue or a bus.
    /// </summary>
    Queue
}
