// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Library.Tests.Frontends.when_removing_an_author;

/// <summary>
/// The library refuses to remove an author the catalog still points at, and answers 409 with problem details. A
/// rejection is an outcome to show, not a crash — so the page must say why and leave the author exactly where it
/// was, in both frontends.
/// </summary>
/// <param name="library">The running composition.</param>
[Trait("Category", "Integration")]
[Collection(LibrarySystem.Name)]
public class and_books_are_attributed_to_them(DistributedLibrary library)
{
    [IntegrationTheory]
    [InlineData(Frontend.Razor)]
    [InlineData(Frontend.React)]
    public async Task should_refuse_and_keep_the_author(Frontend frontend)
    {
        await using var driver = await library.Open(frontend);
        (await driver.FrontendKind()).ShouldEqual(frontend.ToString());

        var authorName = Unique.Name("Ravensworth");
        await driver.RegisterAuthor("Elias", authorName);
        await driver.AddBook(Unique.Isbn(), Unique.Name("Winter Ledger "), await driver.AuthorKey(authorName));

        await driver.RemoveAuthor(authorName);

        (await driver.Rejection()).ShouldNotBeEmpty();
        (await driver.ListsAuthor(authorName)).ShouldBeTrue();
    }
}
