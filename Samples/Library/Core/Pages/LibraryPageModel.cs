// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Library.Core.Pages;

/// <summary>
/// The shape every library page shares: the API it talks to, and the one error message it may have to show.
/// </summary>
/// <param name="api">The <see cref="LibraryApi"/> the page's handlers call.</param>
/// <remarks>
/// Mutations follow post-redirect-get, so a rejection has to survive one redirect to reach the page that renders
/// it. That is what <see cref="CarryError"/> and the temp data behind it are for.
/// </remarks>
public abstract class LibraryPageModel(LibraryApi api) : PageModel
{
    const string ErrorTitleKey = "LibraryErrorTitle";
    const string ErrorDetailKey = "LibraryErrorDetail";

    /// <summary>
    /// Gets the problem to render in the page's message block, if there is one.
    /// </summary>
    public ApiProblem? Error { get; private set; }

    /// <summary>
    /// Gets the library API the page's handlers call.
    /// </summary>
    protected LibraryApi Api { get; } = api;

    /// <inheritdoc/>
    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (HttpMethods.IsGet(Request.Method) && TempData[ErrorTitleKey] is string title)
        {
            Error = new ApiProblem(title, TempData[ErrorDetailKey] as string ?? string.Empty);
        }
    }

    /// <summary>
    /// Shows a problem on the page currently being rendered.
    /// </summary>
    /// <param name="problem">The <see cref="ApiProblem"/> to show.</param>
    protected void ShowError(ApiProblem problem) => Error = problem;

    /// <summary>
    /// Redirects back to the page after a mutation, carrying any problem across so the reloaded page can show it.
    /// </summary>
    /// <param name="problem">The <see cref="ApiProblem"/> the call reported, or <see langword="null"/> when it succeeded.</param>
    /// <returns>A redirect to the page the form was posted from.</returns>
    protected IActionResult AfterCommand(ApiProblem? problem)
    {
        if (problem is not null)
        {
            CarryError(problem);
        }

        return Redirect($"{Request.PathBase}{Request.Path}");
    }

    /// <summary>
    /// Carries a problem across the redirect that follows a mutation.
    /// </summary>
    /// <param name="problem">The <see cref="ApiProblem"/> to carry.</param>
    protected void CarryError(ApiProblem problem)
    {
        TempData[ErrorTitleKey] = problem.Title;
        TempData[ErrorDetailKey] = problem.Detail;
    }
}
