// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Library.Core.Authors;

namespace Library.Core.Catalog;

/// <summary>
/// Represents a title in the library's catalog. The ISBN is the business identifier callers use, while the
/// database keeps an incrementing surrogate key — the shape a great many long-lived systems ended up with.
/// </summary>
public class Book
{
    /// <summary>
    /// Gets or sets the database-generated identifier of the book.
    /// </summary>
    public int BookId { get; set; }

    /// <summary>
    /// Gets or sets the ISBN uniquely identifying the title.
    /// </summary>
    public string Isbn { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the title of the book.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifier of the author who wrote the book.
    /// </summary>
    public int AuthorId { get; set; }

    /// <summary>
    /// Gets or sets the author who wrote the book.
    /// </summary>
    public Author? Author { get; set; }

    /// <summary>
    /// Gets the free-text tags describing the book.
    /// </summary>
    public ICollection<BookTag> Tags { get; } = [];
}
