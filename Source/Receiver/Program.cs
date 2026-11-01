// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using Cratis.Prologue.Storage;
using Cratis.Prologue.Workbench;
using Cratis.Prologue.Workbench.Captures.Recording;
using Cratis.Prologue.Workbench.Model.Export;
using Cratis.Prologue.Workbench.Rebuild.Parity;

// Force invariant culture for the backend
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);

// The Workbench: Cratis.AI, then Arc with Chronicle (event store "Prologue") and MongoDB read models.
builder.AddWorkbench();

builder.Services.AddPrologueCaptureStorage(builder.Configuration);

// Match the Prologue Extractor's transport serialization so concepts and polymorphic payloads round-trip.
builder.Services.ConfigureHttpJsonOptions(options => CaptureSerialization.AddConverters(options.SerializerOptions));

var app = builder.Build();

// Serve the Workbench frontend built into wwwroot.
app.UseDefaultFiles();
app.UseStaticFiles();

// Observable queries reach the browser over the WebSocket hub Arc maps; without this the upgrade is a 500.
app.UseWebSockets();
app.UseCratis();

// Public-facing endpoint the Prologue Extractor posts correlated captures to when it is not associated with a Prologue.
app.MapPost("/captures", async (Capture capture, ICaptureStore store, ICaptureRecordingQueue recordings, CancellationToken cancellationToken) =>
{
    // The capture is stored; recording it for the Workbench happens in the background, so the answer never waits for the event store.
    // A capture stored already is a retry and was recorded the first time.
    if (await store.Store(capture, cancellationToken))
    {
        recordings.TryEnqueue(capture);
    }

    return Results.Accepted();
});

// Endpoint the Prologue Extractor posts to when configured with the Prologue it captures for, so captures are
// associated with that Prologue and can later be analyzed on their own by the interpreter.
app.MapPost("/prologues/{prologueId:guid}/captures", async (Guid prologueId, Capture capture, ICaptureStore store, ICaptureRecordingQueue recordings, CancellationToken cancellationToken) =>
{
    var associated = capture with { PrologueId = prologueId };
    if (await store.Store(associated, cancellationToken))
    {
        recordings.TryEnqueue(associated);
    }

    return Results.Accepted();
});

// Endpoint the Prologue Extractor posts to after it mirrored a command to the shadow system and compared the two answers.
app.MapPost("/prologues/{prologueId:guid}/parity", async (Guid prologueId, ParityObservation observation, IParityRecorder recorder, CancellationToken cancellationToken) =>
{
    await recorder.Record(prologueId, observation, cancellationToken);
    return Results.Accepted();
});

// The Screenplay of the prologue's accepted model, as a download.
app.MapModelScreenplay();

// Any other path that is not a file belongs to the Workbench's client-side router.
app.MapFallbackToFile("/index.html");

await app.RunAsync();
