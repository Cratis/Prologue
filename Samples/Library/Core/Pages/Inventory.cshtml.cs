// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Library.Core.Catalog;
using Library.Core.Inventory;
using Microsoft.AspNetCore.Mvc;

namespace Library.Core.Pages;

/// <summary>
/// How many copies of each title the library holds, and the form that adds more.
/// </summary>
/// <param name="api">The <see cref="LibraryApi"/> the page's handlers call.</param>
public class InventoryModel(LibraryApi api) : LibraryPageModel(api)
{
    /// <summary>
    /// Gets the inventory records.
    /// </summary>
    public IReadOnlyList<InventoryDetails> Items { get; private set; } = [];

    /// <summary>
    /// Gets the titles copies can be registered against.
    /// </summary>
    public IReadOnlyList<BookDetails> Books { get; private set; } = [];

    /// <summary>
    /// Gets or sets the ISBN posted by the register form.
    /// </summary>
    [BindProperty]
    public string Isbn { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets how many copies the register form was filled in with.
    /// </summary>
    [BindProperty]
    public int Count { get; set; } = 1;

    /// <summary>
    /// Gets or sets the ISBN of the title the loss form is writing copies off of.
    /// </summary>
    [BindProperty]
    public string LostIsbn { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets how many copies the loss form was filled in with.
    /// </summary>
    [BindProperty]
    public int LostCount { get; set; } = 1;

    /// <summary>
    /// Loads the inventory and the titles to pick from.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task OnGet(CancellationToken cancellationToken)
    {
        var items = await Api.GetInventory(cancellationToken);
        var books = await Api.GetBooks(cancellationToken);

        Items = items.Value ?? [];
        Books = books.Value ?? [];

        if ((items.Problem ?? books.Problem) is { } problem)
        {
            ShowError(problem);
        }
    }

    /// <summary>
    /// Registers the copies the form was filled in with.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>A redirect back to the page.</returns>
    public async Task<IActionResult> OnPostRegister(CancellationToken cancellationToken)
    {
        var result = await Api.RegisterInventory(new RegisterInventory(Isbn, Count), cancellationToken);

        return AfterCommand(result.Problem);
    }

    /// <summary>
    /// Writes the copies the form was filled in with off as lost.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>A redirect back to the page.</returns>
    public async Task<IActionResult> OnPostLost(CancellationToken cancellationToken)
    {
        var result = await Api.ReportLost(LostIsbn, new ReportLost(LostCount), cancellationToken);

        return AfterCommand(result.Problem);
    }
}
