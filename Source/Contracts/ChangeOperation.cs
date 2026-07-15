// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Contracts;

/// <summary>
/// Represents the kind of change a database transaction applied to a row.
/// </summary>
public enum ChangeOperation
{
    /// <summary>
    /// A row was inserted.
    /// </summary>
    Insert,

    /// <summary>
    /// A row was updated.
    /// </summary>
    Update,

    /// <summary>
    /// A row was deleted.
    /// </summary>
    Delete
}
