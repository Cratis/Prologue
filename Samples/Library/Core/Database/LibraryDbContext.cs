// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Library.Core.Authors;
using Library.Core.Catalog;
using Library.Core.Inventory;
using Library.Core.Loans;
using Library.Core.Members;
using Library.Core.Reservations;
using Microsoft.EntityFrameworkCore;

namespace Library.Core.Database;

/// <summary>
/// Represents the library's relational store. Deliberately a plain <see cref="DbContext"/> with conventional
/// mappings — the point of this sample is to look like an ordinary ASP.NET + EF Core system that Prologue can be
/// pointed at, not to demonstrate anything Cratis. Each entity's mapping lives beside the entity itself.
/// </summary>
/// <param name="options">The options the context is configured with.</param>
public class LibraryDbContext(DbContextOptions<LibraryDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Gets the authors.
    /// </summary>
    public DbSet<Author> Authors => Set<Author>();

    /// <summary>
    /// Gets the members.
    /// </summary>
    public DbSet<Member> Members => Set<Member>();

    /// <summary>
    /// Gets the books in the catalog.
    /// </summary>
    public DbSet<Book> Books => Set<Book>();

    /// <summary>
    /// Gets the free-text tags attached to books.
    /// </summary>
    public DbSet<BookTag> BookTags => Set<BookTag>();

    /// <summary>
    /// Gets the inventory records.
    /// </summary>
    public DbSet<InventoryItem> Inventory => Set<InventoryItem>();

    /// <summary>
    /// Gets the reservations.
    /// </summary>
    public DbSet<Reservation> Reservations => Set<Reservation>();

    /// <summary>
    /// Gets the loans.
    /// </summary>
    public DbSet<Loan> Loans => Set<Loan>();

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LibraryDbContext).Assembly);
}
