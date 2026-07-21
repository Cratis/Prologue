// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Library.Core.Database;
using Library.Core.Telemetry;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Library.Core.Catalog;

/// <summary>
/// Represents the details needed to register a book into the catalog.
/// </summary>
/// <param name="Isbn">The ISBN identifying the title.</param>
/// <param name="Title">The title of the book.</param>
/// <param name="AuthorId">The identifier of the author who wrote it.</param>
public record RegisterBook(string Isbn, string Title, int AuthorId);

/// <summary>
/// Represents the details needed to tag a book.
/// </summary>
/// <param name="Tag">The free-text tag to attach.</param>
public record AddBookTag(string Tag);

/// <summary>
/// Represents a catalog entry as returned to callers.
/// </summary>
/// <param name="BookId">The identifier of the book.</param>
/// <param name="Isbn">The ISBN identifying the title.</param>
/// <param name="Title">The title of the book.</param>
/// <param name="AuthorId">The identifier of the author who wrote it.</param>
/// <param name="AuthorName">The name of the author who wrote it.</param>
/// <param name="Tags">The free-text tags attached to the book.</param>
public record BookDetails(int BookId, string Isbn, string Title, int AuthorId, string AuthorName, IReadOnlyList<string> Tags);

/// <summary>
/// Exposes the library's catalog of titles.
/// </summary>
/// <param name="dbContext">The library database.</param>
[ApiController]
[Route("api/catalog")]
public class CatalogController(LibraryDbContext dbContext) : ControllerBase
{
    /// <summary>
    /// Lists every book in the catalog.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The catalog entries.</returns>
    [HttpGet("books")]
    public async Task<ActionResult<IEnumerable<BookDetails>>> AllBooks(CancellationToken cancellationToken) =>
        await dbContext.Books
            .OrderBy(book => book.Title)
            .Select(book => new BookDetails(
                book.BookId,
                book.Isbn,
                book.Title,
                book.AuthorId,
                book.Author!.FirstName + " " + book.Author.LastName,
                book.Tags.Select(tag => tag.Tag).ToList()))
            .ToListAsync(cancellationToken);

    /// <summary>
    /// Registers a book into the catalog.
    /// </summary>
    /// <param name="command">The book to register.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The registered book.</returns>
    [HttpPost("books")]
    public async Task<ActionResult<BookDetails>> RegisterBook([FromBody] RegisterBook command, CancellationToken cancellationToken)
    {
        var author = await dbContext.Authors.FirstOrDefaultAsync(candidate => candidate.AuthorId == command.AuthorId, cancellationToken);

        if (author is null)
        {
            return UnprocessableEntity(new ProblemDetails
            {
                Title = "Unknown author",
                Detail = $"No author with id {command.AuthorId} is registered.",
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }

        var book = new Book { Isbn = command.Isbn, Title = command.Title, AuthorId = command.AuthorId };

        dbContext.Books.Add(book);
        await dbContext.SaveChangesAsync(cancellationToken);

        LibraryTelemetry.Operations.Add(1, new KeyValuePair<string, object?>("operation", "register-book"));

        var details = new BookDetails(book.BookId, book.Isbn, book.Title, author.AuthorId, $"{author.FirstName} {author.LastName}", []);
        return CreatedAtAction(nameof(AllBooks), new { isbn = book.Isbn }, details);
    }

    /// <summary>
    /// Attaches a free-text tag to a book.
    /// </summary>
    /// <param name="isbn">The ISBN of the book to tag.</param>
    /// <param name="command">The tag to attach.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>Created when attached, not found when the book is unknown.</returns>
    [HttpPost("books/{isbn}/tags")]
    public async Task<IActionResult> AddTag(string isbn, [FromBody] AddBookTag command, CancellationToken cancellationToken)
    {
        var book = await dbContext.Books.FirstOrDefaultAsync(candidate => candidate.Isbn == isbn, cancellationToken);

        if (book is null)
        {
            return NotFound();
        }

        var alreadyTagged = await dbContext.BookTags
            .AnyAsync(tag => tag.BookId == book.BookId && tag.Tag == command.Tag, cancellationToken);

        if (alreadyTagged)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Tag already attached",
                Detail = $"'{command.Tag}' is already attached to {book.Title}.",
                Status = StatusCodes.Status409Conflict
            });
        }

        dbContext.BookTags.Add(new BookTag { BookId = book.BookId, Tag = command.Tag });
        await dbContext.SaveChangesAsync(cancellationToken);

        LibraryTelemetry.Operations.Add(1, new KeyValuePair<string, object?>("operation", "tag-book"));

        return Created($"/api/catalog/books/{isbn}/tags", new { book.Isbn, command.Tag });
    }
}
