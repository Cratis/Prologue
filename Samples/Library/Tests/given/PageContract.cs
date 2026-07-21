// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Library.Tests.given;

/// <summary>
/// The routes both frontends serve. Razor and React deliberately use the same paths, which is what lets the driver
/// navigate without knowing which one it is talking to.
/// </summary>
public static class PagePaths
{
    /// <summary>The authors page.</summary>
    public const string Authors = "/authors";

    /// <summary>The members page.</summary>
    public const string Members = "/members";

    /// <summary>The catalog page.</summary>
    public const string Catalog = "/catalog";

    /// <summary>The inventory page.</summary>
    public const string Inventory = "/inventory";

    /// <summary>The reservations page.</summary>
    public const string Reservations = "/reservations";

    /// <summary>The loans page.</summary>
    public const string Loans = "/loans";

    /// <summary>The simulation page.</summary>
    public const string Simulation = "/simulation";
}

/// <summary>
/// Every <c>data-testid</c> the specs touch, named once. Both frontends render these on the same elements — that
/// agreement is the whole reason a single driver can work against either of them.
/// </summary>
public static class TestIds
{
    /// <summary>The badge naming which frontend is being served.</summary>
    public const string FrontendKind = "frontend-kind";

    /// <summary>The block a rejection is shown in.</summary>
    public const string Error = "error";

    /// <summary>The author first-name field.</summary>
    public const string AuthorFirstName = "author-first-name";

    /// <summary>The author last-name field.</summary>
    public const string AuthorLastName = "author-last-name";

    /// <summary>The button that registers an author.</summary>
    public const string AuthorSubmit = "author-submit";

    /// <summary>The table of registered authors.</summary>
    public const string AuthorsTable = "authors-table";

    /// <summary>A row in the authors table.</summary>
    public const string AuthorRow = "author-row";

    /// <summary>The button that removes an author.</summary>
    public const string AuthorDelete = "author-delete";

    /// <summary>The member first-name field.</summary>
    public const string MemberFirstName = "member-first-name";

    /// <summary>The member last-name field.</summary>
    public const string MemberLastName = "member-last-name";

    /// <summary>The button that registers a member.</summary>
    public const string MemberSubmit = "member-submit";

    /// <summary>The table of registered members.</summary>
    public const string MembersTable = "members-table";

    /// <summary>A row in the members table.</summary>
    public const string MemberRow = "member-row";

    /// <summary>The book ISBN field.</summary>
    public const string BookIsbn = "book-isbn";

    /// <summary>The book title field.</summary>
    public const string BookTitle = "book-title";

    /// <summary>The author picker on the catalog page.</summary>
    public const string BookAuthor = "book-author";

    /// <summary>The button that adds a book to the catalog.</summary>
    public const string BookSubmit = "book-submit";

    /// <summary>The table of catalogued books.</summary>
    public const string BooksTable = "books-table";

    /// <summary>A row in the books table.</summary>
    public const string BookRow = "book-row";

    /// <summary>The title picker on the inventory page.</summary>
    public const string InventoryIsbn = "inventory-isbn";

    /// <summary>The number-of-copies field.</summary>
    public const string InventoryCount = "inventory-count";

    /// <summary>The button that stocks copies.</summary>
    public const string InventorySubmit = "inventory-submit";

    /// <summary>The table of stocked titles.</summary>
    public const string InventoryTable = "inventory-table";

    /// <summary>A row in the inventory table.</summary>
    public const string InventoryRow = "inventory-row";

    /// <summary>The title picker on the reservations page.</summary>
    public const string ReservationIsbn = "reservation-isbn";

    /// <summary>The member picker on the reservations page.</summary>
    public const string ReservationMember = "reservation-member";

    /// <summary>The button that places a reservation.</summary>
    public const string ReservationSubmit = "reservation-submit";

    /// <summary>The table of reservations.</summary>
    public const string ReservationsTable = "reservations-table";

    /// <summary>A row in the reservations table.</summary>
    public const string ReservationRow = "reservation-row";

    /// <summary>The title picker on the loans page.</summary>
    public const string LoanIsbn = "loan-isbn";

    /// <summary>The member picker on the loans page.</summary>
    public const string LoanMember = "loan-member";

    /// <summary>The button that lends a copy out.</summary>
    public const string LoanSubmit = "loan-submit";

    /// <summary>The table of loans.</summary>
    public const string LoansTable = "loans-table";

    /// <summary>A row in the loans table.</summary>
    public const string LoanRow = "loan-row";

    /// <summary>The button that returns an open loan.</summary>
    public const string LoanReturn = "loan-return";
}
