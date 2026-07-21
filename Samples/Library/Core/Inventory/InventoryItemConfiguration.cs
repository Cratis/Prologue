// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Library.Core.Inventory;

/// <summary>
/// Maps <see cref="InventoryItem"/> onto the <c>Inventory</c> table.
/// </summary>
public class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.ToTable("Inventory");
        builder.HasKey(item => item.InventoryItemId);
        builder.HasIndex(item => item.BookId).IsUnique();

        builder.HasOne(item => item.Book)
            .WithMany()
            .HasForeignKey(item => item.BookId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
