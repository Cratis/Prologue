// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Library.Tests.Frontends.when_reserving_a_title;

/// <summary>
/// Reserving needs a copy to reserve. The library is stocked with exactly one, which is then lent out, so the
/// reservation meets the 422 path — the other shape of rejection both frontends have to render the same way.
/// </summary>
/// <param name="library">The running composition.</param>
[Trait("Category", "Integration")]
[Collection(LibrarySystem.Name)]
public class and_every_copy_is_out_on_loan(DistributedLibrary library)
{
    [IntegrationTheory]
    [InlineData(Frontend.Razor)]
    [InlineData(Frontend.React)]
    public async Task should_refuse_the_reservation(Frontend frontend)
    {
        await using var driver = await library.Open(frontend);
        (await driver.FrontendKind()).ShouldEqual(frontend.ToString());

        var stocked = await driver.StockATitle(copies: 1);
        await driver.LendCopy(stocked.Isbn, stocked.MemberKey);
        (await driver.ListsLoan(stocked.Title)).ShouldBeTrue();

        await driver.ReserveTitle(stocked.Isbn, stocked.MemberKey);

        (await driver.Rejection()).ShouldNotBeEmpty();
    }
}
