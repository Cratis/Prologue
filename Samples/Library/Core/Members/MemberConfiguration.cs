// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Library.Core.Members;

/// <summary>
/// Maps <see cref="Member"/> onto the <c>Members</c> table.
/// </summary>
public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.ToTable("Members");
        builder.HasKey(member => member.MemberId);
        builder.Property(member => member.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(member => member.LastName).HasMaxLength(100).IsRequired();
    }
}
