// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Library.Tests.Frontends.when_registering_a_member;

/// <summary>
/// Members are who the library lends to; nothing can be borrowed until one exists.
/// </summary>
/// <param name="library">The running composition.</param>
[Trait("Category", "Integration")]
[Collection(LibrarySystem.Name)]
public class and_the_form_is_submitted(DistributedLibrary library)
{
    [IntegrationTheory]
    [InlineData(Frontend.Razor)]
    [InlineData(Frontend.React)]
    public async Task should_list_the_member(Frontend frontend)
    {
        await using var driver = await library.Open(frontend);
        (await driver.FrontendKind()).ShouldEqual(frontend.ToString());

        var lastName = Unique.Name("Sorenson");
        await driver.RegisterMember("Ada", lastName);

        (await driver.ListsMember(lastName)).ShouldBeTrue();
    }
}
