// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;
using Cratis.Prologue.Configuration;
using Cratis.Prologue.Contracts;
using Cratis.Prologue.Interpretation;
using Cratis.Prologue.Screenplay;
using Cratis.Prologue.Storage;
using Cratis.Screenplay.Printing;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Orleans.Serialization;
using Orleans.Storage;

namespace Cratis.Prologue.Interpreter;

/// <summary>
/// Hosts the interpreter's service mode — an Orleans silo with the session grain and the minimal HTTP API that
/// drives sessions, in one process. Captures load from the capture store rather than mounted files, session state
/// persists to the Prologue MongoDB database, and the lifecycle watcher exits the process cleanly when a session
/// has been awaiting answers beyond the grace period or the service goes idle.
/// </summary>
public static class ServiceHost
{
    /// <summary>
    /// Runs the service until it decides to exit cleanly.
    /// </summary>
    /// <param name="arguments">The parsed <see cref="InterpreterArguments"/>.</param>
    /// <returns>The process exit code.</returns>
    public static async Task<int> Run(InterpreterArguments arguments)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls($"http://*:{arguments.ServicePort}");

        // Configuration comes from the same cratis-prologue.json every Prologue tool uses (path overridable
        // through PROLOGUE_CONFIG), with the environment re-applied on top.
        builder.Configuration.AddPrologueConfiguration(Directory.GetCurrentDirectory());

        builder.Services.AddPrologueCaptureStorage(builder.Configuration);
        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            CaptureSerialization.AddConverters(options.SerializerOptions);
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        builder.Services.AddSingleton(arguments);
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddSingleton<ISessionActivityTracker, SessionActivityTracker>();
        builder.Services.AddHostedService<ServiceLifecycleWatcher>();

        builder.Services.AddSingleton<IBuildHeuristicModel, HeuristicModelBuilder>();
        builder.Services.AddSingleton<IChatClientFactory, ChatClientFactory>();
        builder.Services.AddSingleton<IInterpreterSessionFactory, InterpreterSessionFactory>();
        builder.Services.AddSingleton<IScreenplayPrinter, ScreenplayPrinter>();
        builder.Services.AddSingleton<IScreenplayGenerator, ScreenplayGenerator>();

        // Session state lives in its own collection in the same Prologue MongoDB database the captures are in.
        builder.Services.AddSingleton(serviceProvider =>
        {
            var mongo = serviceProvider.GetRequiredService<IOptions<PrologueStorageOptions>>().Value.Mongo;
            return new MongoClient(mongo.ConnectionString)
                .GetDatabase(mongo.Database)
                .GetCollection<InterpreterSessionStateDocument>(InterpreterSessionStateDocument.CollectionName);
        });

        builder.Host.UseOrleans(silo =>
        {
            silo.UseLocalhostClustering();

            // The session contracts are plain records without Orleans codecs — serialize every Prologue type
            // crossing a grain boundary with System.Text.Json.
            silo.Services.AddSerializer(serialization => serialization.AddJsonSerializer(
                type => type.Namespace?.StartsWith("Cratis.Prologue", StringComparison.Ordinal) == true));

            silo.Services.AddKeyedSingleton<IGrainStorage>(
                WellKnownStorageProviders.Default,
                (serviceProvider, _) => new MongoDbGrainStorage<InterpreterSessionStateDocument>(
                    serviceProvider.GetRequiredService<IMongoCollection<InterpreterSessionStateDocument>>()));
        });

        var app = builder.Build();
        MapEndpoints(app);
        await app.RunAsync();

        return 0;
    }

    static void MapEndpoints(WebApplication app)
    {
        // Reachability probe — how the orchestrator detects whether the container is still around.
        app.MapGet("/healthz", () => Results.Ok());

        // Start (or continue) the session for a Prologue — interpretation proceeds in the background.
        app.MapPost("/sessions/{prologueId:guid}", async (Guid prologueId, LlmOptions options, IGrainFactory grains) =>
        {
            await grains.GetGrain<IInterpreterSessionGrain>(prologueId).Start(options);
            return Results.Accepted();
        });

        // The session's snapshot — status, pending questions, system name and error.
        app.MapGet("/sessions/{prologueId:guid}", async (Guid prologueId, IGrainFactory grains) =>
            Results.Ok(await grains.GetGrain<IInterpreterSessionGrain>(prologueId).GetStatus()));

        // Answer one pending question — continues the session once every pending question is answered.
        app.MapPost("/sessions/{prologueId:guid}/answers", async (Guid prologueId, InterpreterAnswer answer, IGrainFactory grains) =>
        {
            try
            {
                await grains.GetGrain<IInterpreterSessionGrain>(prologueId).Answer(answer);
                return Results.Accepted();
            }
            catch (QuestionNotPending notPending)
            {
                return Results.Problem(
                    detail: notPending.Message,
                    statusCode: StatusCodes.Status404NotFound,
                    title: "The question is not pending");
            }
        });

        // The completed session's extraction result and generated Screenplay.
        app.MapGet("/sessions/{prologueId:guid}/result", async (Guid prologueId, IGrainFactory grains) =>
        {
            var result = await grains.GetGrain<IInterpreterSessionGrain>(prologueId).GetResult();
            return result is null
                ? Results.Problem(statusCode: StatusCodes.Status409Conflict, title: "The session has not completed")
                : Results.Ok(result);
        });
    }
}
