// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Library.Core.Authors;
using Library.Core.Catalog;
using Library.Core.Inventory;
using Library.Core.Loans;
using Library.Core.Members;
using Library.Core.Reservations;
using Library.Core.Simulation;

namespace Library.Core.Pages;

/// <summary>
/// The library API as the Razor pages see it. The page handlers hold no business logic of their own — every
/// mutation is the same REST call the React frontend makes, so the two cannot drift apart.
/// </summary>
/// <param name="transport">The <see cref="ApiTransport"/> the calls travel over.</param>
public class LibraryApi(ApiTransport transport)
{
    /// <summary>
    /// The name of the <see cref="HttpClient"/> the Razor pages reach the library API through.
    /// </summary>
    public const string HttpClientName = "library-api";

    /// <summary>
    /// Lists every registered author.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The registered authors.</returns>
    public Task<ApiResult<IReadOnlyList<AuthorDetails>>> GetAuthors(CancellationToken cancellationToken) =>
        transport.Read<IReadOnlyList<AuthorDetails>>(HttpMethod.Get, "api/authors", null, cancellationToken);

    /// <summary>
    /// Registers an author.
    /// </summary>
    /// <param name="command">The author to register.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The registered author.</returns>
    public Task<ApiResult<AuthorDetails>> RegisterAuthor(RegisterAuthor command, CancellationToken cancellationToken) =>
        transport.Read<AuthorDetails>(HttpMethod.Post, "api/authors", command, cancellationToken);

    /// <summary>
    /// Removes an author, which the API rejects while books are still attributed to them.
    /// </summary>
    /// <param name="authorId">The identifier of the author to remove.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The outcome of the call.</returns>
    public Task<ApiResult> DeleteAuthor(int authorId, CancellationToken cancellationToken) =>
        transport.Send(HttpMethod.Delete, $"api/authors/{authorId}", null, cancellationToken);

    /// <summary>
    /// Lists every registered member.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The registered members.</returns>
    public Task<ApiResult<IReadOnlyList<MemberDetails>>> GetMembers(CancellationToken cancellationToken) =>
        transport.Read<IReadOnlyList<MemberDetails>>(HttpMethod.Get, "api/members", null, cancellationToken);

    /// <summary>
    /// Registers a member.
    /// </summary>
    /// <param name="command">The member to register.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The registered member.</returns>
    public Task<ApiResult<MemberDetails>> RegisterMember(RegisterMember command, CancellationToken cancellationToken) =>
        transport.Read<MemberDetails>(HttpMethod.Post, "api/members", command, cancellationToken);

    /// <summary>
    /// Lists every book in the catalog.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The catalog entries.</returns>
    public Task<ApiResult<IReadOnlyList<BookDetails>>> GetBooks(CancellationToken cancellationToken) =>
        transport.Read<IReadOnlyList<BookDetails>>(HttpMethod.Get, "api/catalog/books", null, cancellationToken);

    /// <summary>
    /// Registers a book into the catalog.
    /// </summary>
    /// <param name="command">The book to register.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The registered book.</returns>
    public Task<ApiResult<BookDetails>> RegisterBook(RegisterBook command, CancellationToken cancellationToken) =>
        transport.Read<BookDetails>(HttpMethod.Post, "api/catalog/books", command, cancellationToken);

    /// <summary>
    /// Attaches a free-text tag to a book, which the API rejects when the tag is already on it.
    /// </summary>
    /// <param name="isbn">The ISBN of the book to tag.</param>
    /// <param name="command">The tag to attach.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The outcome of the call.</returns>
    public Task<ApiResult> AddBookTag(string isbn, AddBookTag command, CancellationToken cancellationToken) =>
        transport.Send(HttpMethod.Post, $"api/catalog/books/{Uri.EscapeDataString(isbn)}/tags", command, cancellationToken);

    /// <summary>
    /// Lists how many copies of each title the library holds.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The inventory records.</returns>
    public Task<ApiResult<IReadOnlyList<InventoryDetails>>> GetInventory(CancellationToken cancellationToken) =>
        transport.Read<IReadOnlyList<InventoryDetails>>(HttpMethod.Get, "api/inventory", null, cancellationToken);

    /// <summary>
    /// Registers copies of a title into the inventory.
    /// </summary>
    /// <param name="command">The copies to register.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The resulting inventory record.</returns>
    public Task<ApiResult<InventoryDetails>> RegisterInventory(RegisterInventory command, CancellationToken cancellationToken) =>
        transport.Read<InventoryDetails>(HttpMethod.Post, "api/inventory", command, cancellationToken);

    /// <summary>
    /// Writes copies of a title off as lost, which the API rejects when more are written off than are on the shelf.
    /// </summary>
    /// <param name="isbn">The ISBN of the title copies were lost of.</param>
    /// <param name="command">How many copies were lost.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The adjusted inventory record, or the reason it was rejected.</returns>
    public Task<ApiResult<InventoryDetails>> ReportLost(string isbn, ReportLost command, CancellationToken cancellationToken) =>
        transport.Read<InventoryDetails>(HttpMethod.Post, $"api/inventory/{Uri.EscapeDataString(isbn)}/lost", command, cancellationToken);

    /// <summary>
    /// Lists the reservations and who made them.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The reservations.</returns>
    public Task<ApiResult<IReadOnlyList<ReservationDetails>>> GetReservations(CancellationToken cancellationToken) =>
        transport.Read<IReadOnlyList<ReservationDetails>>(HttpMethod.Get, "api/reservations", null, cancellationToken);

    /// <summary>
    /// Reserves a copy of a title for a member, which the API rejects when no copies are available.
    /// </summary>
    /// <param name="command">The reservation to make.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The reservation, or the reason it was rejected.</returns>
    public Task<ApiResult<ReservationDetails>> ReserveBook(ReserveBook command, CancellationToken cancellationToken) =>
        transport.Read<ReservationDetails>(HttpMethod.Post, "api/reservations", command, cancellationToken);

    /// <summary>
    /// Lists which copies are out with which members.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The loans.</returns>
    public Task<ApiResult<IReadOnlyList<LoanDetails>>> GetLoans(CancellationToken cancellationToken) =>
        transport.Read<IReadOnlyList<LoanDetails>>(HttpMethod.Get, "api/loans", null, cancellationToken);

    /// <summary>
    /// Lends a copy of a title to a member, which the API rejects when no copies are available.
    /// </summary>
    /// <param name="command">The loan to make.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The loan, or the reason it was rejected.</returns>
    public Task<ApiResult<LoanDetails>> LendBook(LendBook command, CancellationToken cancellationToken) =>
        transport.Read<LoanDetails>(HttpMethod.Post, "api/loans", command, cancellationToken);

    /// <summary>
    /// Returns a lent copy, putting it back on the shelf.
    /// </summary>
    /// <param name="loanId">The identifier of the loan being returned.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The outcome of the call.</returns>
    public Task<ApiResult> ReturnLoan(int loanId, CancellationToken cancellationToken) =>
        transport.Send(HttpMethod.Post, $"api/loans/{loanId}/return", null, cancellationToken);

    /// <summary>
    /// Reports how the current or most recent simulation run is going.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The simulation status.</returns>
    public Task<ApiResult<SimulationStatus>> GetSimulationStatus(CancellationToken cancellationToken) =>
        transport.Read<SimulationStatus>(HttpMethod.Get, "api/simulation/status", null, cancellationToken);

    /// <summary>
    /// Starts a simulation run.
    /// </summary>
    /// <param name="command">How many transactions to carry out.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The outcome of the call.</returns>
    public Task<ApiResult> StartSimulation(StartSimulation command, CancellationToken cancellationToken) =>
        transport.Send(HttpMethod.Post, "api/simulation/start", command, cancellationToken);

    /// <summary>
    /// Stops the simulation run in flight.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The outcome of the call.</returns>
    public Task<ApiResult> StopSimulation(CancellationToken cancellationToken) =>
        transport.Send(HttpMethod.Post, "api/simulation/stop", null, cancellationToken);
}
