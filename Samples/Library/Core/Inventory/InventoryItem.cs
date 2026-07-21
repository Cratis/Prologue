// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Library.Core.Catalog;

namespace Library.Core.Inventory;

/// <summary>
/// Represents how many physical copies of a title the library holds, and how many of those are on the shelf.
/// </summary>
public class InventoryItem
{
    /// <summary>
    /// Gets or sets the database-generated identifier of the inventory record.
    /// </summary>
    public int InventoryItemId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the book the copies are of.
    /// </summary>
    public int BookId { get; set; }

    /// <summary>
    /// Gets or sets the book the copies are of.
    /// </summary>
    public Book? Book { get; set; }

    /// <summary>
    /// Gets or sets how many copies the library owns.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets how many copies are available to reserve or lend right now.
    /// </summary>
    public int AvailableCount { get; set; }
}
