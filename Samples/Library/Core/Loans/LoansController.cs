// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Library.Core.Database;
using Library.Core.Telemetry;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Library.Core.Loans;

/// <summary>
/// Represents the details needed to lend a copy of a title to a member.
/// </summary>
/// <param name="Isbn">The ISBN of the title to lend.</param>
/// <param name="MemberId">The identifier of the member borrowing it.</param>
public record LendBook(string Isbn, int MemberId);

/// <summary>
/// Represents a loan as returned to callers.
/// </summary>
/// <param name="LoanId">The identifier of the loan.</param>
/// <param name="Isbn">The ISBN of the lent title.</param>
/// <param name="Title">The title of the lent book.</param>
/// <param name="MemberId">The identifier of the borrowing member.</param>
/// <param name="MemberName">The name of the borrowing member.</param>
/// <param name="LentOn">When the book was lent out.</param>
/// <param name="ReturnedOn">When the book was returned, if it has been.</param>
public record LoanDetails(int LoanId, string Isbn, string Title, int MemberId, string MemberName, DateTimeOffset LentOn, DateTimeOffset? ReturnedOn);

/// <summary>
/// Exposes which copies are out with which members.
/// </summary>
/// <param name="dbContext">The library database.</param>
[ApiController]
[Route("api/loans")]
public class LoansController(LibraryDbContext dbContext) : ControllerBase
{
    /// <summary>
    /// Lists the books that are out and who has them.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The loans.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LoanDetails>>> All(CancellationToken cancellationToken) =>
        await dbContext.Loans
            .OrderByDescending(loan => loan.LentOn)
            .Select(loan => new LoanDetails(
                loan.LoanId,
                loan.Book!.Isbn,
                loan.Book.Title,
                loan.MemberId,
                loan.Member!.FirstName + " " + loan.Member.LastName,
                loan.LentOn,
                loan.ReturnedOn))
            .ToListAsync(cancellationToken);

    /// <summary>
    /// Lends a copy of a title to a member. The loan and the reduced availability are written in one transaction.
    /// </summary>
    /// <param name="command">The loan to make.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The loan, or unprocessable when no copies are available.</returns>
    [HttpPost]
    public async Task<ActionResult<LoanDetails>> Lend([FromBody] LendBook command, CancellationToken cancellationToken)
    {
        var item = await dbContext.Inventory
            .Include(candidate => candidate.Book)
            .FirstOrDefaultAsync(candidate => candidate.Book!.Isbn == command.Isbn, cancellationToken);

        if (item is null)
        {
            return UnprocessableEntity(Problem("Unknown title", $"No copies of ISBN {command.Isbn} are held."));
        }

        var member = await dbContext.Members.FirstOrDefaultAsync(candidate => candidate.MemberId == command.MemberId, cancellationToken);

        if (member is null)
        {
            return UnprocessableEntity(Problem("Unknown member", $"No member with id {command.MemberId} is registered."));
        }

        if (item.AvailableCount <= 0)
        {
            LibraryTelemetry.Operations.Add(1, new KeyValuePair<string, object?>("operation", "lend-rejected"));

            return UnprocessableEntity(Problem("No copies available", $"All copies of {item.Book!.Title} are currently out."));
        }

        item.AvailableCount--;

        var loan = new Loan
        {
            BookId = item.BookId,
            MemberId = member.MemberId,
            LentOn = DateTimeOffset.UtcNow
        };

        dbContext.Loans.Add(loan);
        await dbContext.SaveChangesAsync(cancellationToken);

        LibraryTelemetry.Operations.Add(1, new KeyValuePair<string, object?>("operation", "lend"));

        var details = new LoanDetails(
            loan.LoanId,
            item.Book!.Isbn,
            item.Book.Title,
            member.MemberId,
            $"{member.FirstName} {member.LastName}",
            loan.LentOn,
            null);

        return CreatedAtAction(nameof(All), new { id = loan.LoanId }, details);
    }

    /// <summary>
    /// Returns a lent copy, putting it back on the shelf.
    /// </summary>
    /// <param name="id">The identifier of the loan being returned.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The closed loan.</returns>
    [HttpPost("{id:int}/return")]
    public async Task<ActionResult<LoanDetails>> Return(int id, CancellationToken cancellationToken)
    {
        var loan = await dbContext.Loans
            .Include(candidate => candidate.Book)
            .Include(candidate => candidate.Member)
            .FirstOrDefaultAsync(candidate => candidate.LoanId == id, cancellationToken);

        if (loan is null)
        {
            return NotFound();
        }

        if (loan.ReturnedOn is not null)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Already returned",
                Detail = $"Loan {id} was returned on {loan.ReturnedOn:u}.",
                Status = StatusCodes.Status409Conflict
            });
        }

        loan.ReturnedOn = DateTimeOffset.UtcNow;

        var item = await dbContext.Inventory.FirstOrDefaultAsync(candidate => candidate.BookId == loan.BookId, cancellationToken);

        if (item is { } returned)
        {
            returned.AvailableCount++;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        LibraryTelemetry.Operations.Add(1, new KeyValuePair<string, object?>("operation", "return"));

        return new LoanDetails(
            loan.LoanId,
            loan.Book!.Isbn,
            loan.Book.Title,
            loan.MemberId,
            $"{loan.Member!.FirstName} {loan.Member.LastName}",
            loan.LentOn,
            loan.ReturnedOn);
    }

    static ProblemDetails Problem(string title, string detail) =>
        new() { Title = title, Detail = detail, Status = StatusCodes.Status422UnprocessableEntity };
}
