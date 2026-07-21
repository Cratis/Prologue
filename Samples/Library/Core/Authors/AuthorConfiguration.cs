// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Library.Core.Authors;

/// <summary>
/// Maps <see cref="Author"/> onto the <c>Authors</c> table.
/// </summary>
public class AuthorConfiguration : IEntityTypeConfiguration<Author>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Author> builder)
    {
        builder.ToTable("Authors");
        builder.HasKey(author => author.AuthorId);
        builder.Property(author => author.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(author => author.LastName).HasMaxLength(100).IsRequired();
    }
}
