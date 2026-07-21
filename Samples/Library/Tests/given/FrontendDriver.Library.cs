// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;

namespace Library.Tests.given;

/// <summary>
/// What a librarian does, expressed once for both frontends. Each flow navigates to the page that owns it, so a
/// spec reads as the sequence of things a person would do rather than as a sequence of clicks.
/// </summary>
public sealed partial class FrontendDriver
{
    /// <summary>
    /// Registers an author.
    /// </summary>
    /// <param name="firstName">The author's first name.</param>
    /// <param name="lastName">The author's last name.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task RegisterAuthor(string firstName, string lastName)
    {
        await GoTo(PagePaths.Authors);
        await Fill(TestIds.AuthorFirstName, firstName);
        await Fill(TestIds.AuthorLastName, lastName);
        await Press(TestIds.AuthorSubmit);
    }

    /// <summary>
    /// Determines whether an author is listed.
    /// </summary>
    /// <param name="name">Any part of the author's name.</param>
    /// <returns>True when the author is listed.</returns>
    public Task<bool> ListsAuthor(string name) => HasRow(TestIds.AuthorsTable, TestIds.AuthorRow, name);

    /// <summary>
    /// Reads the key of a listed author, which is what the catalog's author picker is populated with.
    /// </summary>
    /// <param name="name">Any part of the author's name.</param>
    /// <returns>The author's key.</returns>
    public Task<string> AuthorKey(string name) => KeyOf(TestIds.AuthorsTable, TestIds.AuthorRow, name);

    /// <summary>
    /// Removes an author, which the library refuses while books are attributed to them.
    /// </summary>
    /// <param name="name">Any part of the author's name.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task RemoveAuthor(string name)
    {
        await GoTo(PagePaths.Authors);
        await PressInRow(TestIds.AuthorsTable, TestIds.AuthorRow, name, TestIds.AuthorDelete);
    }

    /// <summary>
    /// Registers a member.
    /// </summary>
    /// <param name="firstName">The member's first name.</param>
    /// <param name="lastName">The member's last name.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task RegisterMember(string firstName, string lastName)
    {
        await GoTo(PagePaths.Members);
        await Fill(TestIds.MemberFirstName, firstName);
        await Fill(TestIds.MemberLastName, lastName);
        await Press(TestIds.MemberSubmit);
    }

    /// <summary>
    /// Determines whether a member is listed.
    /// </summary>
    /// <param name="name">Any part of the member's name.</param>
    /// <returns>True when the member is listed.</returns>
    public Task<bool> ListsMember(string name) => HasRow(TestIds.MembersTable, TestIds.MemberRow, name);

    /// <summary>
    /// Reads the key of a listed member, which is what the loan and reservation member pickers are populated with.
    /// </summary>
    /// <param name="name">Any part of the member's name.</param>
    /// <returns>The member's key.</returns>
    public Task<string> MemberKey(string name) => KeyOf(TestIds.MembersTable, TestIds.MemberRow, name);

    /// <summary>
    /// Adds a book to the catalog.
    /// </summary>
    /// <param name="isbn">The book's ISBN.</param>
    /// <param name="title">The book's title.</param>
    /// <param name="authorKey">The key of the author the book is attributed to.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task AddBook(string isbn, string title, string authorKey)
    {
        await GoTo(PagePaths.Catalog);
        await Fill(TestIds.BookIsbn, isbn);
        await Fill(TestIds.BookTitle, title);
        await Pick(TestIds.BookAuthor, authorKey);
        await Press(TestIds.BookSubmit);
    }

    /// <summary>
    /// Determines whether a book is catalogued.
    /// </summary>
    /// <param name="titleOrIsbn">The book's title or ISBN.</param>
    /// <returns>True when the book is catalogued.</returns>
    public Task<bool> ListsBook(string titleOrIsbn) => HasRow(TestIds.BooksTable, TestIds.BookRow, titleOrIsbn);

    /// <summary>
    /// Stocks copies of a catalogued title.
    /// </summary>
    /// <param name="isbn">The ISBN of the title to stock.</param>
    /// <param name="copies">How many copies the library holds.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task StockCopies(string isbn, int copies)
    {
        await GoTo(PagePaths.Inventory);
        await Pick(TestIds.InventoryIsbn, isbn);
        await Fill(TestIds.InventoryCount, copies.ToString(CultureInfo.InvariantCulture));
        await Press(TestIds.InventorySubmit);
    }

    /// <summary>
    /// Lends a copy of a title to a member.
    /// </summary>
    /// <param name="isbn">The ISBN of the title to lend.</param>
    /// <param name="memberKey">The key of the member borrowing it.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task LendCopy(string isbn, string memberKey)
    {
        await GoTo(PagePaths.Loans);
        await Pick(TestIds.LoanIsbn, isbn);
        await Pick(TestIds.LoanMember, memberKey);
        await Press(TestIds.LoanSubmit);
    }

    /// <summary>
    /// Determines whether a loan is listed.
    /// </summary>
    /// <param name="titleOrIsbn">The lent title or its ISBN.</param>
    /// <returns>True when the loan is listed.</returns>
    public Task<bool> ListsLoan(string titleOrIsbn) => HasRow(TestIds.LoansTable, TestIds.LoanRow, titleOrIsbn);

    /// <summary>
    /// Determines whether a listed loan is still open, which is what offering a return button means.
    /// </summary>
    /// <param name="titleOrIsbn">The lent title or its ISBN.</param>
    /// <returns>True when the loan is still out.</returns>
    public Task<bool> LoanIsOpen(string titleOrIsbn) =>
        RowOffers(TestIds.LoansTable, TestIds.LoanRow, titleOrIsbn, TestIds.LoanReturn);

    /// <summary>
    /// Returns a lent copy.
    /// </summary>
    /// <param name="titleOrIsbn">The lent title or its ISBN.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task ReturnCopy(string titleOrIsbn)
    {
        await GoTo(PagePaths.Loans);
        await PressInRow(TestIds.LoansTable, TestIds.LoanRow, titleOrIsbn, TestIds.LoanReturn);

        // Wait until the row stops offering to return the copy, which is the page saying it has taken the return
        // in. The server-rendered frontend redirects and so is always current by the time it renders, but the
        // single-page one re-fetches after the fact — asking whether the loan is still open the instant the click
        // returns would read the previous answer. Waiting here keeps that difference out of the specs.
        await Settles(Row(TestIds.LoansTable, TestIds.LoanRow, titleOrIsbn).GetByTestId(TestIds.LoanReturn));
    }

    /// <summary>
    /// Reserves a title for a member.
    /// </summary>
    /// <param name="isbn">The ISBN of the title to reserve.</param>
    /// <param name="memberKey">The key of the member reserving it.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task ReserveTitle(string isbn, string memberKey)
    {
        await GoTo(PagePaths.Reservations);
        await Pick(TestIds.ReservationIsbn, isbn);
        await Pick(TestIds.ReservationMember, memberKey);
        await Press(TestIds.ReservationSubmit);
    }
}
