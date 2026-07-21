// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Library.Core.Catalog;
using Library.Core.Loans;
using Library.Core.Members;
using Microsoft.AspNetCore.Mvc;

namespace Library.Core.Pages;

/// <summary>
/// Which copies are out with which members, and the forms that lend and take them back.
/// </summary>
/// <param name="api">The <see cref="LibraryApi"/> the page's handlers call.</param>
public class LoansModel(LibraryApi api) : LibraryPageModel(api)
{
    /// <summary>
    /// Gets the loans.
    /// </summary>
    public IReadOnlyList<LoanDetails> Loans { get; private set; } = [];

    /// <summary>
    /// Gets the titles that can be lent.
    /// </summary>
    public IReadOnlyList<BookDetails> Books { get; private set; } = [];

    /// <summary>
    /// Gets the members a copy can be lent to.
    /// </summary>
    public IReadOnlyList<MemberDetails> Members { get; private set; } = [];

    /// <summary>
    /// Gets or sets the ISBN posted by the lend form.
    /// </summary>
    [BindProperty]
    public string Isbn { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the member identifier posted by the lend form.
    /// </summary>
    [BindProperty]
    public int MemberId { get; set; }

    /// <summary>
    /// Gets or sets the loan identifier posted by a row's return form.
    /// </summary>
    [BindProperty]
    public int LoanId { get; set; }

    /// <summary>
    /// Loads the loans and what a new one can be made from.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task OnGet(CancellationToken cancellationToken)
    {
        var loans = await Api.GetLoans(cancellationToken);
        var books = await Api.GetBooks(cancellationToken);
        var members = await Api.GetMembers(cancellationToken);

        Loans = loans.Value ?? [];
        Books = books.Value ?? [];
        Members = members.Value ?? [];

        if ((loans.Problem ?? books.Problem ?? members.Problem) is { } problem)
        {
            ShowError(problem);
        }
    }

    /// <summary>
    /// Lends the title the form was filled in with.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>A redirect back to the page.</returns>
    public async Task<IActionResult> OnPostLend(CancellationToken cancellationToken)
    {
        var result = await Api.LendBook(new LendBook(Isbn, MemberId), cancellationToken);

        return AfterCommand(result.Problem);
    }

    /// <summary>
    /// Returns the copy whose row the return button was pressed in.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>A redirect back to the page.</returns>
    public async Task<IActionResult> OnPostReturn(CancellationToken cancellationToken)
    {
        var result = await Api.ReturnLoan(LoanId, cancellationToken);

        return AfterCommand(result.Problem);
    }
}
