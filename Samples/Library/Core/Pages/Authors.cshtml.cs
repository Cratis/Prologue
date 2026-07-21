// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Library.Core.Authors;
using Microsoft.AspNetCore.Mvc;

namespace Library.Core.Pages;

/// <summary>
/// The people the library holds books by, and the forms that register and remove them.
/// </summary>
/// <param name="api">The <see cref="LibraryApi"/> the page's handlers call.</param>
public class AuthorsModel(LibraryApi api) : LibraryPageModel(api)
{
    /// <summary>
    /// Gets the registered authors.
    /// </summary>
    public IReadOnlyList<AuthorDetails> Authors { get; private set; } = [];

    /// <summary>
    /// Gets or sets the first name posted by the register form.
    /// </summary>
    [BindProperty]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last name posted by the register form.
    /// </summary>
    [BindProperty]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifier posted by a row's delete form.
    /// </summary>
    [BindProperty]
    public int AuthorId { get; set; }

    /// <summary>
    /// Loads the authors to show.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task OnGet(CancellationToken cancellationToken)
    {
        var result = await Api.GetAuthors(cancellationToken);

        Authors = result.Value ?? [];

        if (result.Problem is { } problem)
        {
            ShowError(problem);
        }
    }

    /// <summary>
    /// Registers the author the form was filled in with.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>A redirect back to the page.</returns>
    public async Task<IActionResult> OnPostRegister(CancellationToken cancellationToken)
    {
        var result = await Api.RegisterAuthor(new RegisterAuthor(FirstName, LastName), cancellationToken);

        return AfterCommand(result.Problem);
    }

    /// <summary>
    /// Removes the author whose row the delete button was pressed in.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>A redirect back to the page.</returns>
    public async Task<IActionResult> OnPostDelete(CancellationToken cancellationToken)
    {
        var result = await Api.DeleteAuthor(AuthorId, cancellationToken);

        return AfterCommand(result.Problem);
    }
}
