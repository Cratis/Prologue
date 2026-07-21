// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Library.Core.Catalog;

/// <summary>
/// Maps <see cref="Book"/> onto the <c>Books</c> table.
/// </summary>
public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("Books");
        builder.HasKey(book => book.BookId);
        builder.Property(book => book.Isbn).HasMaxLength(20).IsRequired();
        builder.Property(book => book.Title).HasMaxLength(200).IsRequired();

        // The ISBN is the identifier callers use, even though the primary key is the surrogate.
        builder.HasIndex(book => book.Isbn).IsUnique();

        builder.HasOne(book => book.Author)
            .WithMany(author => author.Books)
            .HasForeignKey(book => book.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

/// <summary>
/// Maps <see cref="BookTag"/> onto the <c>BookTags</c> table.
/// </summary>
public class BookTagConfiguration : IEntityTypeConfiguration<BookTag>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<BookTag> builder)
    {
        builder.ToTable("BookTags");
        builder.HasKey(tag => tag.BookTagId);
        builder.Property(tag => tag.Tag).HasMaxLength(50).IsRequired();
        builder.HasIndex(tag => new { tag.BookId, tag.Tag }).IsUnique();

        builder.HasOne(tag => tag.Book)
            .WithMany(book => book.Tags)
            .HasForeignKey(tag => tag.BookId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
