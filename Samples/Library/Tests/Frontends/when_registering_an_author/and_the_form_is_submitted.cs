// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Library.Tests.Frontends.when_registering_an_author;

/// <summary>
/// Registering an author is the simplest write the library offers, and the one everything else builds on.
/// </summary>
/// <param name="library">The running composition.</param>
[Trait("Category", "Integration")]
[Collection(LibrarySystem.Name)]
public class and_the_form_is_submitted(DistributedLibrary library)
{
    [IntegrationTheory]
    [InlineData(Frontend.Razor)]
    [InlineData(Frontend.React)]
    public async Task should_list_the_author(Frontend frontend)
    {
        await using var driver = await library.Open(frontend);
        (await driver.FrontendKind()).ShouldEqual(frontend.ToString());

        var lastName = Unique.Name("Blackwood");
        await driver.RegisterAuthor("Iris", lastName);

        (await driver.ListsAuthor(lastName)).ShouldBeTrue();
    }
}
