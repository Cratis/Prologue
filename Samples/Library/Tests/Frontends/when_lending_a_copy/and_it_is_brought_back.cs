// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Library.Tests.Frontends.when_lending_a_copy;

/// <summary>
/// The full round trip: the only copy goes out, comes back, and can go out again. Lending the same title a second
/// time is what proves the copy really went back on the shelf rather than the loan merely being marked closed.
/// </summary>
/// <param name="library">The running composition.</param>
[Trait("Category", "Integration")]
[Collection(LibrarySystem.Name)]
public class and_it_is_brought_back(DistributedLibrary library)
{
    [IntegrationTheory]
    [InlineData(Frontend.Razor)]
    [InlineData(Frontend.React)]
    public async Task should_lend_it_out_and_take_it_back(Frontend frontend)
    {
        await using var driver = await library.Open(frontend);
        (await driver.FrontendKind()).ShouldEqual(frontend.ToString());

        var stocked = await driver.StockATitle(copies: 1);

        await driver.LendCopy(stocked.Isbn, stocked.MemberKey);
        (await driver.ListsLoan(stocked.Title)).ShouldBeTrue();
        (await driver.LoanIsOpen(stocked.Title)).ShouldBeTrue();

        await driver.ReturnCopy(stocked.Title);
        (await driver.LoanIsOpen(stocked.Title)).ShouldBeFalse();

        await driver.LendCopy(stocked.Isbn, stocked.MemberKey);
        (await driver.Accepted()).ShouldBeTrue();
    }
}
