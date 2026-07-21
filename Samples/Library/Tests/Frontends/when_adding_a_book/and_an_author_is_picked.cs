// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Library.Tests.Frontends.when_adding_a_book;

/// <summary>
/// A book belongs to an author, so cataloguing one crosses two pages: the author registered on one is picked from
/// the other. The author is identified by the key its row carries, which is what both pickers are populated with.
/// </summary>
/// <param name="library">The running composition.</param>
[Trait("Category", "Integration")]
[Collection(LibrarySystem.Name)]
public class and_an_author_is_picked(DistributedLibrary library)
{
    [IntegrationTheory]
    [InlineData(Frontend.Razor)]
    [InlineData(Frontend.React)]
    public async Task should_list_the_book(Frontend frontend)
    {
        await using var driver = await library.Open(frontend);
        (await driver.FrontendKind()).ShouldEqual(frontend.ToString());

        var authorName = Unique.Name("Calloway");
        await driver.RegisterAuthor("Nora", authorName);

        var isbn = Unique.Isbn();
        var title = Unique.Name("Salt and Iron ");
        await driver.AddBook(isbn, title, await driver.AuthorKey(authorName));

        (await driver.ListsBook(title)).ShouldBeTrue();
    }
}
