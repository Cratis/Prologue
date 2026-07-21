// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using Cratis.Prologue.Configuration;
using Cratis.Prologue.Contracts;
using Cratis.Prologue.Interpreter;
using Cratis.Prologue.Interpreter.Contracts;
using Microsoft.Extensions.AI;
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
    await Console.Error.WriteLineAsync("Usage: interpreter [--captures <folder>] [--output <file>] [--prologue-id <guid>]");
    await Console.Error.WriteLineAsync($"Defaults: --captures {InterpreterArguments.DefaultCapturesFolder}, --output {InterpreterArguments.DefaultOutputFile}.");
    await Console.Error.WriteLineAsync("Environment overrides: PROLOGUE_CAPTURES, PROLOGUE_OUTPUT, PROLOGUE_ID, PROLOGUE_CONFIG.");
    return 2;
}

var builder = Host.CreateApplicationBuilder(args);

// Configuration comes from a dedicated cratis-prologue.json file (path overridable through the PROLOGUE_CONFIG
// environment variable) — not appsettings.json. The interpreter binds the Llm section for optional refinement.
builder.Configuration.AddPrologueConfiguration(Directory.GetCurrentDirectory());

builder.Services.AddSingleton<IBuildHeuristicModel, HeuristicModelBuilder>();
builder.Services.AddSingleton<IExtractEventModel, EventModelExtractor>();

// The deterministic heuristics decide the structure; the language model refines the names. When no model is
// configured the interpreter returns the deterministic structure unchanged.
builder.Services.Configure<LlmOptions>(builder.Configuration.GetSection(LlmOptions.SectionName));
var llmOptions = builder.Configuration.GetSection(LlmOptions.SectionName).Get<LlmOptions>() ?? new LlmOptions();
if (llmOptions.Enabled)
{
    builder.Services.AddSingleton(LlmChatClient.CreateFor(llmOptions));
    builder.Services.AddSingleton<IRefineExtraction, LlmExtractionRefiner>();
}
else
{
    builder.Services.AddSingleton<IRefineExtraction, PassthroughExtractionRefiner>();
}

using var host = builder.Build();

if (!Directory.Exists(arguments.CapturesFolder))
{
    await Console.Error.WriteLineAsync($"Captures folder '{arguments.CapturesFolder}' does not exist.");
    return 2;
}

// Run to completion: read the capture files the Extractor produced from the mounted folder, interpret them into
// an event model and write the extraction result to the mounted output location.
var captures = await CaptureFiles.ReadFromFolder(arguments.CapturesFolder, arguments.PrologueId);
await Console.Out.WriteLineAsync($"Read {captures.Count} capture(s) from '{arguments.CapturesFolder}'.");

var extractor = host.Services.GetRequiredService<IExtractEventModel>();
var result = await extractor.Extract(arguments.PrologueId, captures);

await ExtractionResultFile.WriteToFile(result, arguments.OutputFile);
await Console.Out.WriteLineAsync($"Extraction result with {result.Modules.Count} module(s) written to '{arguments.OutputFile}'.");
return 0;
