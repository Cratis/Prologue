// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Library.Core.Database;
using Library.Core.Telemetry;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Library.Core.Reservations;

/// <summary>
/// Represents the details needed to reserve a copy of a title for a member.
/// </summary>
/// <param name="Isbn">The ISBN of the title to reserve.</param>
/// <param name="MemberId">The identifier of the member reserving it.</param>
public record ReserveBook(string Isbn, int MemberId);

/// <summary>
/// Represents a reservation as returned to callers.
/// </summary>
/// <param name="ReservationId">The identifier of the reservation.</param>
/// <param name="Isbn">The ISBN of the reserved title.</param>
/// <param name="Title">The title of the reserved book.</param>
/// <param name="MemberId">The identifier of the member who reserved it.</param>
/// <param name="MemberName">The name of the member who reserved it.</param>
/// <param name="ReservedOn">When the reservation was made.</param>
public record ReservationDetails(int ReservationId, string Isbn, string Title, int MemberId, string MemberName, DateTimeOffset ReservedOn);

/// <summary>
/// Exposes members' claims on copies of titles.
/// </summary>
/// <param name="dbContext">The library database.</param>
[ApiController]
[Route("api/reservations")]
public class ReservationsController(LibraryDbContext dbContext) : ControllerBase
{
    /// <summary>
    /// Lists the reservations and who made them.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The reservations.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReservationDetails>>> All(CancellationToken cancellationToken) =>
        await dbContext.Reservations
            .OrderByDescending(reservation => reservation.ReservedOn)
            .Select(reservation => new ReservationDetails(
                reservation.ReservationId,
                reservation.Book!.Isbn,
                reservation.Book.Title,
                reservation.MemberId,
                reservation.Member!.FirstName + " " + reservation.Member.LastName,
                reservation.ReservedOn))
            .ToListAsync(cancellationToken);

    /// <summary>
    /// Reserves a copy of a title for a member, provided one is available.
    /// </summary>
    /// <param name="command">The reservation to make.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The reservation, or unprocessable when no copies are available.</returns>
    [HttpPost]
    public async Task<ActionResult<ReservationDetails>> Reserve([FromBody] ReserveBook command, CancellationToken cancellationToken)
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

        // The rule the whole slice exists for: you cannot claim a copy that is not on the shelf.
        if (item.AvailableCount <= 0)
        {
            LibraryTelemetry.Operations.Add(1, new KeyValuePair<string, object?>("operation", "reserve-rejected"));

            return UnprocessableEntity(Problem(
                "No copies available",
                $"All copies of {item.Book!.Title} are currently out."));
        }

        item.AvailableCount--;

        var reservation = new Reservation
        {
            BookId = item.BookId,
            MemberId = member.MemberId,
            ReservedOn = DateTimeOffset.UtcNow
        };

        dbContext.Reservations.Add(reservation);
        await dbContext.SaveChangesAsync(cancellationToken);

        LibraryTelemetry.Operations.Add(1, new KeyValuePair<string, object?>("operation", "reserve"));

        var details = new ReservationDetails(
            reservation.ReservationId,
            item.Book!.Isbn,
            item.Book.Title,
            member.MemberId,
            $"{member.FirstName} {member.LastName}",
            reservation.ReservedOn);

        return CreatedAtAction(nameof(All), new { id = reservation.ReservationId }, details);
    }

    static ProblemDetails Problem(string title, string detail) =>
        new() { Title = title, Detail = detail, Status = StatusCodes.Status422UnprocessableEntity };
}
