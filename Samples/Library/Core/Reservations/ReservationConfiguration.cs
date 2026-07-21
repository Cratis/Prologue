// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Library.Core.Reservations;

/// <summary>
/// Maps <see cref="Reservation"/> onto the <c>Reservations</c> table.
/// </summary>
public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("Reservations");
        builder.HasKey(reservation => reservation.ReservationId);

        builder.HasOne(reservation => reservation.Book)
            .WithMany()
            .HasForeignKey(reservation => reservation.BookId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(reservation => reservation.Member)
            .WithMany()
            .HasForeignKey(reservation => reservation.MemberId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
