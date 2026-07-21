// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Library.Core.Authors;
using Library.Core.Catalog;
using Microsoft.AspNetCore.Mvc;

namespace Library.Core.Pages;

/// <summary>
/// The titles the library has in its catalog, and the form that registers them against an author.
/// </summary>
/// <param name="api">The <see cref="LibraryApi"/> the page's handlers call.</param>
public class CatalogModel(LibraryApi api) : LibraryPageModel(api)
{
    /// <summary>
    /// Gets the catalog entries.
    /// </summary>
    public IReadOnlyList<BookDetails> Books { get; private set; } = [];

    /// <summary>
    /// Gets the authors a new title can be attributed to.
    /// </summary>
    public IReadOnlyList<AuthorDetails> Authors { get; private set; } = [];

    /// <summary>
    /// Gets or sets the ISBN posted by the register form.
    /// </summary>
    [BindProperty]
    public string Isbn { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the title posted by the register form.
    /// </summary>
    [BindProperty]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the author identifier posted by the register form.
    /// </summary>
    [BindProperty]
    public int AuthorId { get; set; }

    /// <summary>
    /// Gets or sets the ISBN of the book the tag form is tagging.
    /// </summary>
    [BindProperty]
    public string TagIsbn { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tag posted by the tag form.
    /// </summary>
    [BindProperty]
    public string TagValue { get; set; } = string.Empty;

    /// <summary>
    /// Loads the catalog and the authors to pick from.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task OnGet(CancellationToken cancellationToken)
    {
        var books = await Api.GetBooks(cancellationToken);
        var authors = await Api.GetAuthors(cancellationToken);

        Books = books.Value ?? [];
        Authors = authors.Value ?? [];

        if ((books.Problem ?? authors.Problem) is { } problem)
        {
            ShowError(problem);
        }
    }

    /// <summary>
    /// Registers the title the form was filled in with.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>A redirect back to the page.</returns>
    public async Task<IActionResult> OnPostRegister(CancellationToken cancellationToken)
    {
        var result = await Api.RegisterBook(new RegisterBook(Isbn, Title, AuthorId), cancellationToken);

        return AfterCommand(result.Problem);
    }

    /// <summary>
    /// Attaches the tag the form was filled in with to the chosen book.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>A redirect back to the page.</returns>
    public async Task<IActionResult> OnPostTag(CancellationToken cancellationToken)
    {
        var result = await Api.AddBookTag(TagIsbn, new AddBookTag(TagValue), cancellationToken);

        return AfterCommand(result.Problem);
    }
}
