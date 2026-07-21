// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Library.Core.Database;
using Library.Core.Telemetry;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Library.Core.Inventory;

/// <summary>
/// Represents the details needed to register copies of a title into the inventory.
/// </summary>
/// <param name="Isbn">The ISBN of the title copies are held of.</param>
/// <param name="Count">How many copies are being registered.</param>
public record RegisterInventory(string Isbn, int Count);

/// <summary>
/// Represents the details needed to write copies off as lost.
/// </summary>
/// <param name="Count">How many copies were lost.</param>
public record ReportLost(int Count);

/// <summary>
/// Represents an inventory record as returned to callers, enriched with the title from the catalog.
/// </summary>
/// <param name="Isbn">The ISBN of the title.</param>
/// <param name="Title">The title of the book.</param>
/// <param name="AuthorName">The name of the author who wrote it.</param>
/// <param name="TotalCount">How many copies the library owns.</param>
/// <param name="AvailableCount">How many copies are available right now.</param>
public record InventoryDetails(string Isbn, string Title, string AuthorName, int TotalCount, int AvailableCount);

/// <summary>
/// Exposes how many copies of each title the library holds.
/// </summary>
/// <param name="dbContext">The library database.</param>
[ApiController]
[Route("api/inventory")]
public class InventoryController(LibraryDbContext dbContext) : ControllerBase
{
    /// <summary>
    /// Lists the inventory, with the title details coming from the catalog.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The inventory records.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<InventoryDetails>>> All(CancellationToken cancellationToken) =>
        await dbContext.Inventory
            .OrderBy(item => item.Book!.Title)
            .Select(item => new InventoryDetails(
                item.Book!.Isbn,
                item.Book.Title,
                item.Book.Author!.FirstName + " " + item.Book.Author.LastName,
                item.TotalCount,
                item.AvailableCount))
            .ToListAsync(cancellationToken);

    /// <summary>
    /// Registers copies of a title into the inventory, adding to the count when it is already held.
    /// </summary>
    /// <param name="command">The copies to register.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The resulting inventory record.</returns>
    [HttpPost]
    public async Task<ActionResult<InventoryDetails>> Register([FromBody] RegisterInventory command, CancellationToken cancellationToken)
    {
        var book = await dbContext.Books
            .Include(candidate => candidate.Author)
            .FirstOrDefaultAsync(candidate => candidate.Isbn == command.Isbn, cancellationToken);

        if (book is null)
        {
            return UnprocessableEntity(new ProblemDetails
            {
                Title = "Unknown title",
                Detail = $"No book with ISBN {command.Isbn} is in the catalog.",
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }

        var item = await dbContext.Inventory.FirstOrDefaultAsync(candidate => candidate.BookId == book.BookId, cancellationToken);

        if (item is null)
        {
            item = new InventoryItem { BookId = book.BookId, TotalCount = command.Count, AvailableCount = command.Count };
            dbContext.Inventory.Add(item);
        }
        else
        {
            item.TotalCount += command.Count;
            item.AvailableCount += command.Count;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        LibraryTelemetry.Operations.Add(1, new KeyValuePair<string, object?>("operation", "register-inventory"));

        var details = ToDetails(book, item);
        return CreatedAtAction(nameof(All), new { isbn = book.Isbn }, details);
    }

    /// <summary>
    /// Writes copies of a title off as lost, reducing both the owned and available counts.
    /// </summary>
    /// <param name="isbn">The ISBN of the title copies were lost of.</param>
    /// <param name="command">How many copies were lost.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The adjusted inventory record.</returns>
    [HttpPost("{isbn}/lost")]
    public async Task<ActionResult<InventoryDetails>> ReportLost(string isbn, [FromBody] ReportLost command, CancellationToken cancellationToken)
    {
        var item = await dbContext.Inventory
            .Include(candidate => candidate.Book).ThenInclude(book => book!.Author)
            .FirstOrDefaultAsync(candidate => candidate.Book!.Isbn == isbn, cancellationToken);

        if (item is null)
        {
            return NotFound();
        }

        if (command.Count > item.AvailableCount)
        {
            return UnprocessableEntity(new ProblemDetails
            {
                Title = "More copies lost than are on the shelf",
                Detail = $"{item.AvailableCount} copy/copies of {item.Book!.Title} are available; cannot write off {command.Count}.",
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }

        item.TotalCount -= command.Count;
        item.AvailableCount -= command.Count;

        await dbContext.SaveChangesAsync(cancellationToken);

        LibraryTelemetry.Operations.Add(1, new KeyValuePair<string, object?>("operation", "report-lost"));

        return ToDetails(item.Book!, item);
    }

    static InventoryDetails ToDetails(Catalog.Book book, InventoryItem item) =>
        new(book.Isbn, book.Title, $"{book.Author?.FirstName} {book.Author?.LastName}".Trim(), item.TotalCount, item.AvailableCount);
}
