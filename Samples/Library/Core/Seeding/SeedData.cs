// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Library.Core.Authors;
using Library.Core.Catalog;
using Library.Core.Database;
using Library.Core.Inventory;
using Library.Core.Members;
using Microsoft.EntityFrameworkCore;

namespace Library.Core.Seeding;

/// <summary>
/// Writes the starting state of the library — a small, believable collection so the system has something to show
/// and the simulation has something to work with before it starts generating traffic of its own.
/// </summary>
/// <param name="dbContext">The library database.</param>
/// <param name="logger">The logger.</param>
public class SeedData(LibraryDbContext dbContext, ILogger<SeedData> logger)
{
    /// <summary>
    /// Applies the seed data, doing nothing when the library already holds authors.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task Apply(CancellationToken cancellationToken)
    {
        if (await dbContext.Authors.AnyAsync(cancellationToken))
        {
            return;
        }

        var authors = SeedCatalog.Authors.Select(seed => new Author { FirstName = seed.FirstName, LastName = seed.LastName }).ToList();
        dbContext.Authors.AddRange(authors);

        var members = SeedCatalog.Members.Select(seed => new Member { FirstName = seed.FirstName, LastName = seed.LastName }).ToList();
        dbContext.Members.AddRange(members);

        await dbContext.SaveChangesAsync(cancellationToken);

        var books = SeedCatalog.Books
            .Select((seed, index) => new Book
            {
                Isbn = seed.Isbn,
                Title = seed.Title,
                AuthorId = authors[index % authors.Count].AuthorId
            })
            .ToList();

        dbContext.Books.AddRange(books);
        await dbContext.SaveChangesAsync(cancellationToken);

        foreach (var (book, index) in books.Select((book, index) => (book, index)))
        {
            var tags = SeedCatalog.Tags[index % SeedCatalog.Tags.Length];
            dbContext.BookTags.AddRange(tags.Select(tag => new BookTag { BookId = book.BookId, Tag = tag }));

            var copies = 2 + (index % 4);
            dbContext.Inventory.Add(new InventoryItem { BookId = book.BookId, TotalCount = copies, AvailableCount = copies });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        SeedDataLog.Seeded(logger, authors.Count, members.Count, books.Count);
    }
}
