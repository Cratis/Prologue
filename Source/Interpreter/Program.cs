// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using Cratis.Prologue.Configuration;
using Cratis.Prologue.Contracts;
using Cratis.Prologue.Interpretation;
using Cratis.Prologue.Interpreter;
using Cratis.Prologue.Interpreter.Contracts;
using Cratis.Prologue.Screenplay;
using Cratis.Screenplay.Printing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Force invariant culture for the backend.
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

var arguments = InterpreterArguments.Parse(args);
if (arguments is null)
{
    await Console.Error.WriteLineAsync("Usage: interpreter [--captures <folder>] [--output <file>] [--play-output <file>] [--prologue-id <guid>] [--serve]");
    await Console.Error.WriteLineAsync($"Defaults: --captures {InterpreterArguments.DefaultCapturesFolder}, --output {InterpreterArguments.DefaultOutputFile}, --play-output derived from the output folder and the system name.");
    await Console.Error.WriteLineAsync("Environment overrides: PROLOGUE_CAPTURES, PROLOGUE_OUTPUT, PROLOGUE_PLAY_OUTPUT, PROLOGUE_ID, PROLOGUE_CONFIG.");
    await Console.Error.WriteLineAsync("Service mode: --serve or PROLOGUE_MODE=service, tuned with PROLOGUE_SERVICE_PORT, PROLOGUE_GRACE_PERIOD and PROLOGUE_IDLE_TIMEOUT (seconds).");
    return 2;
}

// Service mode hosts resumable sessions over HTTP for Studio — captures come from the capture store and the
// session state persists to MongoDB, so the process can exit at any time and resume later.
if (arguments.Serve)
{
    return await ServiceHost.Run(arguments);
}

var builder = Host.CreateApplicationBuilder(args);

// Configuration comes from a dedicated cratis-prologue.json file (path overridable through the PROLOGUE_CONFIG
// environment variable) — not appsettings.json. The interpreter binds the Llm section for optional refinement.
builder.Configuration.AddPrologueConfiguration(Directory.GetCurrentDirectory());

builder.Services.AddSingleton<IBuildHeuristicModel, HeuristicModelBuilder>();
builder.Services.AddSingleton<IChatClientFactory, ChatClientFactory>();
builder.Services.AddSingleton<IInterpreterSessionFactory, InterpreterSessionFactory>();
builder.Services.AddSingleton<IScreenplayPrinter, ScreenplayPrinter>();
builder.Services.AddSingleton<IScreenplayGenerator, ScreenplayGenerator>();

var llmOptions = builder.Configuration.GetSection(LlmOptions.SectionName).Get<LlmOptions>() ?? new LlmOptions();

using var host = builder.Build();

if (!Directory.Exists(arguments.CapturesFolder))
{
    await Console.Error.WriteLineAsync($"Captures folder '{arguments.CapturesFolder}' does not exist.");
    return 2;
}

// Run to completion: read the capture files the Extractor produced from the mounted folder, interpret them into
// an event model and write the extraction result to the mounted output location. Batch mode is non-interactive —
// the session runs with zero question rounds, so the language model is told not to ask questions and the session
// finalizes with its best judgment instead of parking to await answers.
var captures = await CaptureFiles.ReadFromFolder(arguments.CapturesFolder, arguments.PrologueId);
await Console.Out.WriteLineAsync($"Read {captures.Count} capture(s) from '{arguments.CapturesFolder}'.");

var factory = host.Services.GetRequiredService<IInterpreterSessionFactory>();
var callbacks = new BatchCallbacks();
var session = factory.CreateNew(arguments.PrologueId, captures, llmOptions, callbacks.OnStatusChanged, maxQuestionRounds: 0);
var state = await new InterpreterRunner().Run(session, callbacks);

if (state.Status == InterpreterStatus.Failed)
{
    await Console.Error.WriteLineAsync($"Interpretation failed: {state.Error}");
    return 1;
}

var result = state.Model ?? ExtractionResult.Empty(arguments.PrologueId);
await ExtractionResultFile.WriteToFile(result, arguments.OutputFile);
await Console.Out.WriteLineAsync($"Extraction result for '{result.SystemName}' with {result.Modules.Count} module(s) written to '{arguments.OutputFile}'.");

// Generate the Screenplay next to the extraction result — the .play document a developer continues authoring
// from. The session itself never enters this stage, so the host reports it while emitting the output.
callbacks.OnStatusChanged(InterpreterStatus.GeneratingScreenplay);
var play = host.Services.GetRequiredService<IScreenplayGenerator>().Generate(result);
var playFile = arguments.PlayOutputFor(result.SystemName);
if (Path.GetDirectoryName(playFile) is { Length: > 0 } playFolder)
{
    Directory.CreateDirectory(playFolder);
}

await File.WriteAllTextAsync(playFile, play);
await Console.Out.WriteLineAsync($"Screenplay written to '{playFile}'.");
return 0;
