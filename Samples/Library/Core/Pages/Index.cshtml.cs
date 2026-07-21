// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Library.Core.Pages;

/// <summary>
/// The root of the Razor frontend, which has nothing of its own to show.
/// </summary>
public class IndexModel : PageModel
{
    /// <summary>
    /// Sends the visitor on to the first page that does.
    /// </summary>
    /// <returns>A redirect to the authors page.</returns>
    public IActionResult OnGet() => RedirectToPage("/Authors");
}
