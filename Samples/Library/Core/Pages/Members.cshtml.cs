// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Library.Core.Members;
using Microsoft.AspNetCore.Mvc;

namespace Library.Core.Pages;

/// <summary>
/// The people registered with the library, and the form that registers them.
/// </summary>
/// <param name="api">The <see cref="LibraryApi"/> the page's handlers call.</param>
public class MembersModel(LibraryApi api) : LibraryPageModel(api)
{
    /// <summary>
    /// Gets the registered members.
    /// </summary>
    public IReadOnlyList<MemberDetails> Members { get; private set; } = [];

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
    /// Loads the members to show.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task OnGet(CancellationToken cancellationToken)
    {
        var result = await Api.GetMembers(cancellationToken);

        Members = result.Value ?? [];

        if (result.Problem is { } problem)
        {
            ShowError(problem);
        }
    }

    /// <summary>
    /// Registers the member the form was filled in with.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>A redirect back to the page.</returns>
    public async Task<IActionResult> OnPostRegister(CancellationToken cancellationToken)
    {
        var result = await Api.RegisterMember(new RegisterMember(FirstName, LastName), cancellationToken);

        return AfterCommand(result.Problem);
    }
}
