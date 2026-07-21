// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net;
using Library.Tests.Extraction.given;

namespace Library.Tests.Extraction.when_simulating_load;

/// <summary>
/// The end-to-end proof: load driven through the extractor's proxy is carried out by the library, and the extractor
/// correlates what it saw of it into captures the Receiver stores. A small run on purpose — the point is that the
/// pipeline works, not how much of it can be pushed through.
/// </summary>
/// <param name="library">The running composition.</param>
[Trait("Category", "Integration")]
[Collection(LibrarySystem.Name)]
public class and_the_run_completes(DistributedLibrary library)
{
    const int TransactionCount = 200;

    static readonly TimeSpan _runPatience = TimeSpan.FromMinutes(5);
    static readonly TimeSpan _capturePatience = TimeSpan.FromMinutes(3);

    [IntegrationFact]
    public async Task should_account_for_every_transaction_and_capture_the_run()
    {
        using var client = library.CreateProxyClient();

        // Whatever a neighboring spec left in flight is not this spec's run.
        await client.Stop();

        var alreadyStored = await library.Captures.Identifiers(DistributedLibrary.PrologueId);

        using (var accepted = await client.Start(TransactionCount))
        {
            accepted.StatusCode.ShouldEqual(HttpStatusCode.Accepted);
        }

        var finished = await client.WaitUntilDone(_runPatience);

        finished.IsRunning.ShouldBeFalse();
        finished.Requested.ShouldEqual(TransactionCount);
        finished.Failed.ShouldEqual(0);
        (finished.Succeeded + finished.Rejected).ShouldEqual(TransactionCount);

        var captured = await library.Captures.WaitForNew(
            DistributedLibrary.PrologueId,
            alreadyStored,
            fresh => fresh.Carry<HttpCommandObserved>() && fresh.Carry<TelemetryObserved>(),
            _capturePatience);

        captured.ShouldNotBeEmpty();
        captured.Carry<HttpCommandObserved>().ShouldBeTrue();
        captured.Carry<TelemetryObserved>().ShouldBeTrue();
    }
}
