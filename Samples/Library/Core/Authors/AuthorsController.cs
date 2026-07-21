// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Library.Core.Database;
using Library.Core.Telemetry;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Library.Core.Authors;

/// <summary>
/// Exposes the authors the library holds books by.
/// </summary>
/// <param name="dbContext">The library database.</param>
[ApiController]
[Route("api/authors")]
public class AuthorsController(LibraryDbContext dbContext) : ControllerBase
{
    /// <summary>
    /// Lists every registered author.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The registered authors.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AuthorDetails>>> All(CancellationToken cancellationToken) =>
        await dbContext.Authors
            .OrderBy(author => author.LastName).ThenBy(author => author.FirstName)
            .Select(author => new AuthorDetails(author.AuthorId, author.FirstName, author.LastName, author.Books.Count))
            .ToListAsync(cancellationToken);

    /// <summary>
    /// Registers an author.
    /// </summary>
    /// <param name="command">The author to register.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The registered author.</returns>
    [HttpPost]
    public async Task<ActionResult<AuthorDetails>> Register([FromBody] RegisterAuthor command, CancellationToken cancellationToken)
    {
        var author = new Author { FirstName = command.FirstName, LastName = command.LastName };

        dbContext.Authors.Add(author);
        await dbContext.SaveChangesAsync(cancellationToken);

        LibraryTelemetry.Operations.Add(1, new KeyValuePair<string, object?>("operation", "register-author"));

        var details = new AuthorDetails(author.AuthorId, author.FirstName, author.LastName, 0);
        return CreatedAtAction(nameof(All), new { id = author.AuthorId }, details);
    }

    /// <summary>
    /// Removes an author, provided no books in the catalog are attributed to them.
    /// </summary>
    /// <param name="id">The identifier of the author to remove.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>No content when removed, conflict when the author still has books.</returns>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remove(int id, CancellationToken cancellationToken)
    {
        var author = await dbContext.Authors.FirstOrDefaultAsync(candidate => candidate.AuthorId == id, cancellationToken);

        if (author is null)
        {
            return NotFound();
        }

        var bookCount = await dbContext.Books.CountAsync(book => book.AuthorId == id, cancellationToken);

        if (bookCount > 0)
        {
            LibraryTelemetry.Operations.Add(1, new KeyValuePair<string, object?>("operation", "remove-author-blocked"));

            return Conflict(new ProblemDetails
            {
                Title = "Author still has books",
                Detail = $"{author.FirstName} {author.LastName} has {bookCount} book(s) in the catalog and cannot be removed.",
                Status = StatusCodes.Status409Conflict
            });
        }

        dbContext.Authors.Remove(author);
        await dbContext.SaveChangesAsync(cancellationToken);

        LibraryTelemetry.Operations.Add(1, new KeyValuePair<string, object?>("operation", "remove-author"));

        return NoContent();
    }
}
