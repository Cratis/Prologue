// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net;
using Library.Tests.Extraction.given;

namespace Library.Tests.Extraction.when_simulating_load;

/// <summary>
/// Only one run at a time. The run is deliberately big enough that it cannot have finished before the second
/// request lands, and is stopped again before the assertions so a failure here does not leave load running for
/// every spec that follows.
/// </summary>
/// <param name="library">The running composition.</param>
[Trait("Category", "Integration")]
[Collection(LibrarySystem.Name)]
public class and_a_run_is_already_in_flight(DistributedLibrary library)
{
    const int LongRun = 5_000;

    [IntegrationFact]
    public async Task should_refuse_to_start_a_second_run()
    {
        using var client = library.CreateProxyClient();

        await client.Stop();

        using var first = await client.Start(LongRun);
        using var second = await client.Start(LongRun);

        var accepted = first.StatusCode;
        var refused = second.StatusCode;

        await client.Stop();

        accepted.ShouldEqual(HttpStatusCode.Accepted);
        refused.ShouldEqual(HttpStatusCode.Conflict);
    }
}
