// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Library.Core.Catalog;

/// <summary>
/// Represents a free-text tag attached to a book, used to describe or group titles beyond their formal metadata.
/// </summary>
public class BookTag
{
    /// <summary>
    /// Gets or sets the database-generated identifier of the tag.
    /// </summary>
    public int BookTagId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the book the tag belongs to.
    /// </summary>
    public int BookId { get; set; }

    /// <summary>
    /// Gets or sets the book the tag belongs to.
    /// </summary>
    public Book? Book { get; set; }

    /// <summary>
    /// Gets or sets the tag itself.
    /// </summary>
    public string Tag { get; set; } = string.Empty;
}
