// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Library.Core.Catalog;
using Library.Core.Members;
using Library.Core.Reservations;
using Microsoft.AspNetCore.Mvc;

namespace Library.Core.Pages;

/// <summary>
/// The members' claims on copies of titles, and the form that makes them. Reserving a title whose copies are all
/// out is the rejection this page exists to show.
/// </summary>
/// <param name="api">The <see cref="LibraryApi"/> the page's handlers call.</param>
public class ReservationsModel(LibraryApi api) : LibraryPageModel(api)
{
    /// <summary>
    /// Gets the reservations.
    /// </summary>
    public IReadOnlyList<ReservationDetails> Reservations { get; private set; } = [];

    /// <summary>
    /// Gets the titles that can be reserved.
    /// </summary>
    public IReadOnlyList<BookDetails> Books { get; private set; } = [];

    /// <summary>
    /// Gets the members a reservation can be made for.
    /// </summary>
    public IReadOnlyList<MemberDetails> Members { get; private set; } = [];

    /// <summary>
    /// Gets or sets the ISBN posted by the reserve form.
    /// </summary>
    [BindProperty]
    public string Isbn { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the member identifier posted by the reserve form.
    /// </summary>
    [BindProperty]
    public int MemberId { get; set; }

    /// <summary>
    /// Loads the reservations and what a new one can be made from.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task OnGet(CancellationToken cancellationToken)
    {
        var reservations = await Api.GetReservations(cancellationToken);
        var books = await Api.GetBooks(cancellationToken);
        var members = await Api.GetMembers(cancellationToken);

        Reservations = reservations.Value ?? [];
        Books = books.Value ?? [];
        Members = members.Value ?? [];

        if ((reservations.Problem ?? books.Problem ?? members.Problem) is { } problem)
        {
            ShowError(problem);
        }
    }

    /// <summary>
    /// Reserves the title the form was filled in with.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>A redirect back to the page.</returns>
    public async Task<IActionResult> OnPostReserve(CancellationToken cancellationToken)
    {
        var result = await Api.ReserveBook(new ReserveBook(Isbn, MemberId), cancellationToken);

        return AfterCommand(result.Problem);
    }
}
