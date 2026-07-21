// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Library.Core.Catalog;

namespace Library.Core.Authors;

/// <summary>
/// Represents someone who wrote books held by the library.
/// </summary>
public class Author
{
    /// <summary>
    /// Gets or sets the database-generated identifier of the author.
    /// </summary>
    public int AuthorId { get; set; }

    /// <summary>
    /// Gets or sets the author's first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the author's last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets the books in the catalog written by this author.
    /// </summary>
    public ICollection<Book> Books { get; } = [];
}
