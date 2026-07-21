// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Library.Tests.given;

/// <summary>
/// A title the library actually holds copies of, and someone to lend it to.
/// </summary>
/// <param name="Isbn">The title's ISBN.</param>
/// <param name="Title">The title.</param>
/// <param name="AuthorName">The name of the author it is attributed to.</param>
/// <param name="MemberName">The name of the member who can borrow it.</param>
/// <param name="MemberKey">The key of that member, as the pickers are populated with.</param>
public record StockedTitle(string Isbn, string Title, string AuthorName, string MemberName, string MemberKey);

/// <summary>
/// Puts the library into the state borrowing scenarios start from. Every step goes through the frontend, so the
/// setup itself is the same for both and exercises the same API the behavior under test does.
/// </summary>
public static class Stocking
{
    /// <summary>
    /// Registers an author, catalogs a book by them, stocks copies of it, and registers a member to borrow it.
    /// </summary>
    /// <param name="driver">The <see cref="FrontendDriver"/> to do it through.</param>
    /// <param name="copies">How many copies the library holds.</param>
    /// <returns>The <see cref="StockedTitle"/> that is now on the shelf.</returns>
    public static async Task<StockedTitle> StockATitle(this FrontendDriver driver, int copies)
    {
        var authorName = Unique.Name("Wrenfield");
        var memberName = Unique.Name("Halloway");
        var title = Unique.Name("The Long Quiet ");
        var isbn = Unique.Isbn();

        await driver.RegisterAuthor("Marta", authorName);
        var authorKey = await driver.AuthorKey(authorName);

        await driver.AddBook(isbn, title, authorKey);
        await driver.StockCopies(isbn, copies);

        await driver.RegisterMember("Peder", memberName);
        var memberKey = await driver.MemberKey(memberName);

        return new StockedTitle(isbn, title, authorName, memberName, memberKey);
    }
}
