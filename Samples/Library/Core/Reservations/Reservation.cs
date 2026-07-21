// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Library.Core.Catalog;
using Library.Core.Members;

namespace Library.Core.Reservations;

/// <summary>
/// Represents a member's claim on a copy of a title, held until it is collected.
/// </summary>
public class Reservation
{
    /// <summary>
    /// Gets or sets the database-generated identifier of the reservation.
    /// </summary>
    public int ReservationId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the reserved book.
    /// </summary>
    public int BookId { get; set; }

    /// <summary>
    /// Gets or sets the reserved book.
    /// </summary>
    public Book? Book { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the member who reserved it.
    /// </summary>
    public int MemberId { get; set; }

    /// <summary>
    /// Gets or sets the member who reserved it.
    /// </summary>
    public Member? Member { get; set; }

    /// <summary>
    /// Gets or sets when the reservation was made.
    /// </summary>
    public DateTimeOffset ReservedOn { get; set; }
}
