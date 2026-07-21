// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using Library.Core.Database;
using Library.Core.Pages;
using Library.Core.Simulation;
using Library.Core.Telemetry;

// Force invariant culture so the system behaves the same wherever it runs.
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddLibraryDatabase();

// Controllers, not minimal APIs — a controller-per-area API is what the systems Prologue is pointed at look like,
// and the action filter below gives every action its own span.
builder.Services
    .AddControllersWithViews(options => options.Filters.Add<ControllerActionTracing>())
    .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase);

builder.Services.AddRazorPages();
builder.AddRazorFrontend();

// The simulated load must travel through the Prologue Extractor's reverse proxy, otherwise the HTTP commands it
// generates are never seen. The composition points this at the extractor; on its own it falls back to the system's
// own address, which still exercises the system but captures no HTTP.
builder.Services.Configure<SimulationOptions>(builder.Configuration.GetSection(SimulationOptions.SectionName));
builder.Services.AddSingleton<SimulationRunner>();
builder.Services.AddHttpClient(SimulationOptions.HttpClientName, (serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<SimulationOptions>>().Value;
    var address = string.IsNullOrWhiteSpace(options.BaseAddress)
        ? builder.Configuration["ASPNETCORE_URLS"]?.Split(';')[0] ?? "http://localhost:5080"
        : options.BaseAddress;

    client.BaseAddress = new Uri(address.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// The React frontend is served from its own origin by Vite, so it needs to be allowed to call this one.
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.SetIsOriginAllowed(_ => true).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseCors();

app.MapDefaultEndpoints();
app.MapControllers();
app.MapRazorPages();

await app.RunAsync();
