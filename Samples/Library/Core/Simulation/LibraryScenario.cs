// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using Library.Core.Authors;
using Library.Core.Catalog;
using Library.Core.Inventory;
using Library.Core.Loans;
using Library.Core.Members;
using Library.Core.Reservations;

namespace Library.Core.Simulation;

/// <summary>
/// Carries out one plausible thing a library does, chosen at random from a weighted mix. Reads outnumber writes,
/// lending dominates the writes, and a slice of the traffic is deliberately doomed — reserving a title that is all
/// out, deleting an author who still has books — because a capture of a real system contains rejections, and a
/// simulation without them would be a misleadingly tidy one.
/// </summary>
/// <param name="client">The client the traffic is sent through — the Prologue Extractor's reverse proxy.</param>
public sealed class LibraryScenario(HttpClient client)
{
    readonly ConcurrentBag<int> _authorIds = [];
    readonly ConcurrentBag<int> _memberIds = [];
    readonly ConcurrentBag<string> _isbns = [];
    readonly ConcurrentBag<int> _openLoanIds = [];
    int _isbnSequence;

    /// <summary>
    /// Represents how a simulated transaction turned out.
    /// </summary>
    public enum Outcome
    {
        /// <summary>The operation succeeded.</summary>
        Succeeded,

        /// <summary>A business rule rejected the operation — an expected outcome worth capturing.</summary>
        Rejected,

        /// <summary>The operation failed unexpectedly.</summary>
        Failed
    }

    /// <summary>
    /// Loads the state the library already holds, so the simulation acts on real authors, members, and titles
    /// rather than inventing identifiers that do not exist.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task Prime(CancellationToken cancellationToken)
    {
        foreach (var author in await client.GetFromJsonAsync<List<AuthorDetails>>("/api/authors", cancellationToken) ?? [])
        {
            _authorIds.Add(author.AuthorId);
        }

        foreach (var member in await client.GetFromJsonAsync<List<MemberDetails>>("/api/members", cancellationToken) ?? [])
        {
            _memberIds.Add(member.MemberId);
        }

        foreach (var book in await client.GetFromJsonAsync<List<BookDetails>>("/api/catalog/books", cancellationToken) ?? [])
        {
            _isbns.Add(book.Isbn);
        }
    }

    /// <summary>
    /// Carries out a single transaction.
    /// </summary>
    /// <param name="random">The source of randomness for choosing the operation.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>How the transaction turned out.</returns>
    public async Task<Outcome> ExecuteOne(Random random, CancellationToken cancellationToken)
    {
        var roll = random.Next(100);

        return roll switch
        {
            < 20 => await Read(random, cancellationToken),
            < 48 => await Lend(random, cancellationToken),
            < 60 => await Reserve(random, cancellationToken),
            < 70 => await ReturnLoan(random, cancellationToken),
            < 78 => await RegisterBook(random, cancellationToken),
            < 84 => await TagBook(random, cancellationToken),
            < 89 => await RegisterAuthor(random, cancellationToken),
            < 93 => await RegisterMember(random, cancellationToken),
            < 97 => await RegisterInventory(random, cancellationToken),
            _ => await ReportLost(random, cancellationToken),
        };
    }

    static Outcome OutcomeOf(HttpResponseMessage response) => response.StatusCode switch
    {
        HttpStatusCode.UnprocessableEntity or HttpStatusCode.Conflict or HttpStatusCode.NotFound => Outcome.Rejected,
        _ when response.IsSuccessStatusCode => Outcome.Succeeded,
        _ => Outcome.Failed,
    };

    static T? PickFrom<T>(ConcurrentBag<T> pool, Random random)
    {
        var snapshot = pool.ToArray();
        return snapshot.Length == 0 ? default : snapshot[random.Next(snapshot.Length)];
    }

    async Task<Outcome> Read(Random random, CancellationToken cancellationToken)
    {
        var path = random.Next(5) switch
        {
            0 => "/api/authors",
            1 => "/api/members",
            2 => "/api/catalog/books",
            3 => "/api/inventory",
            _ => "/api/loans",
        };

        using var response = await client.GetAsync(path, cancellationToken);
        return OutcomeOf(response);
    }

    async Task<Outcome> RegisterAuthor(Random random, CancellationToken cancellationToken)
    {
        var command = new RegisterAuthor($"Author{random.Next(100_000)}", $"Surname{random.Next(100_000)}");
        using var response = await client.PostAsJsonAsync("/api/authors", command, cancellationToken);

        if (response.IsSuccessStatusCode &&
            await response.Content.ReadFromJsonAsync<AuthorDetails>(cancellationToken) is { } author)
        {
            _authorIds.Add(author.AuthorId);
        }

        return OutcomeOf(response);
    }

    async Task<Outcome> RegisterMember(Random random, CancellationToken cancellationToken)
    {
        var command = new RegisterMember($"Member{random.Next(100_000)}", $"Surname{random.Next(100_000)}");
        using var response = await client.PostAsJsonAsync("/api/members", command, cancellationToken);

        if (response.IsSuccessStatusCode &&
            await response.Content.ReadFromJsonAsync<MemberDetails>(cancellationToken) is { } member)
        {
            _memberIds.Add(member.MemberId);
        }

        return OutcomeOf(response);
    }

    async Task<Outcome> RegisterBook(Random random, CancellationToken cancellationToken)
    {
        var authorId = PickFrom(_authorIds, random);

        if (authorId == 0)
        {
            return await RegisterAuthor(random, cancellationToken);
        }

        var isbn = NextIsbn();
        var command = new RegisterBook(isbn, $"Title {random.Next(1_000_000)}", authorId);
        using var response = await client.PostAsJsonAsync("/api/catalog/books", command, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            _isbns.Add(isbn);
        }

        return OutcomeOf(response);
    }

    async Task<Outcome> TagBook(Random random, CancellationToken cancellationToken)
    {
        var isbn = PickFrom(_isbns, random);

        if (isbn is null)
        {
            return await RegisterBook(random, cancellationToken);
        }

        var tag = SimulationVocabulary.Tags[random.Next(SimulationVocabulary.Tags.Length)];
        using var response = await client.PostAsJsonAsync($"/api/catalog/books/{isbn}/tags", new AddBookTag(tag), cancellationToken);
        return OutcomeOf(response);
    }

    async Task<Outcome> RegisterInventory(Random random, CancellationToken cancellationToken)
    {
        var isbn = PickFrom(_isbns, random);

        if (isbn is null)
        {
            return await RegisterBook(random, cancellationToken);
        }

        using var response = await client.PostAsJsonAsync("/api/inventory", new RegisterInventory(isbn, 1 + random.Next(4)), cancellationToken);
        return OutcomeOf(response);
    }

    async Task<Outcome> Reserve(Random random, CancellationToken cancellationToken)
    {
        var isbn = PickFrom(_isbns, random);
        var memberId = PickFrom(_memberIds, random);

        if (isbn is null || memberId == 0)
        {
            return Outcome.Rejected;
        }

        using var response = await client.PostAsJsonAsync("/api/reservations", new ReserveBook(isbn, memberId), cancellationToken);
        return OutcomeOf(response);
    }

    async Task<Outcome> Lend(Random random, CancellationToken cancellationToken)
    {
        var isbn = PickFrom(_isbns, random);
        var memberId = PickFrom(_memberIds, random);

        if (isbn is null || memberId == 0)
        {
            return Outcome.Rejected;
        }

        using var response = await client.PostAsJsonAsync("/api/loans", new LendBook(isbn, memberId), cancellationToken);

        if (response.IsSuccessStatusCode &&
            await response.Content.ReadFromJsonAsync<LoanDetails>(cancellationToken) is { } loan)
        {
            _openLoanIds.Add(loan.LoanId);
        }

        return OutcomeOf(response);
    }

    async Task<Outcome> ReturnLoan(Random random, CancellationToken cancellationToken)
    {
        if (!_openLoanIds.TryTake(out var loanId))
        {
            return await Lend(random, cancellationToken);
        }

        using var response = await client.PostAsJsonAsync($"/api/loans/{loanId}/return", new { }, cancellationToken);
        return OutcomeOf(response);
    }

    async Task<Outcome> ReportLost(Random random, CancellationToken cancellationToken)
    {
        var isbn = PickFrom(_isbns, random);

        if (isbn is null)
        {
            return Outcome.Rejected;
        }

        using var response = await client.PostAsJsonAsync($"/api/inventory/{isbn}/lost", new ReportLost(1), cancellationToken);
        return OutcomeOf(response);
    }

    string NextIsbn()
    {
        var next = Interlocked.Increment(ref _isbnSequence);
        return string.Create(CultureInfo.InvariantCulture, $"978{DateTime.UtcNow.Ticks % 1_000_000:D6}{next:D4}");
    }
}
