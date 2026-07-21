// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Library.Core.Loans;

/// <summary>
/// Maps <see cref="Loan"/> onto the <c>Loans</c> table.
/// </summary>
public class LoanConfiguration : IEntityTypeConfiguration<Loan>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder.ToTable("Loans");
        builder.HasKey(loan => loan.LoanId);

        builder.HasOne(loan => loan.Book)
            .WithMany()
            .HasForeignKey(loan => loan.BookId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(loan => loan.Member)
            .WithMany()
            .HasForeignKey(loan => loan.MemberId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
