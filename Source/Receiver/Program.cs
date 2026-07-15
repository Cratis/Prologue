// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using Cratis.Prologue.Storage;

// Force invariant culture for the backend
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPrologueCaptureStorage(builder.Configuration);

// Match the Prologue Extractor's transport serialization so concepts and polymorphic payloads round-trip.
builder.Services.ConfigureHttpJsonOptions(options => CaptureSerialization.AddConverters(options.SerializerOptions));

var app = builder.Build();

// Public-facing endpoint the Prologue Extractor posts correlated captures to when it is not associated with a Prologue.
app.MapPost("/captures", async (Capture capture, ICaptureStore store, CancellationToken cancellationToken) =>
{
    await store.Store(capture, cancellationToken);
    return Results.Accepted();
});

// Endpoint the Prologue Extractor posts to when configured with the Prologue it captures for, so captures are
// associated with that Prologue and can later be analyzed on their own by the interpreter.
app.MapPost("/prologues/{prologueId:guid}/captures", async (Guid prologueId, Capture capture, ICaptureStore store, CancellationToken cancellationToken) =>
{
    await store.Store(capture with { PrologueId = prologueId }, cancellationToken);
    return Results.Accepted();
});

await app.RunAsync();
