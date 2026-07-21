// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using Cratis.Prologue.Storage;
using Library.Composition;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Library.Tests.given;

/// <summary>
/// The whole composition started once and shared by every spec: the library system, both frontends, the Prologue
/// Extractor in front of them, the Receiver behind it, MongoDB, and the library database. Starting it pulls
/// container images and installs the frontend's packages, so it is measured in minutes — which is exactly why it is
/// a collection fixture rather than something each spec stands up for itself.
/// </summary>
public sealed class DistributedLibrary : IAsyncLifetime
{
    /// <summary>
    /// The name of the resource the library system runs as.
    /// </summary>
    public const string CoreResource = "core";

    /// <summary>
    /// The name of the resource the React frontend runs as.
    /// </summary>
    public const string WebResource = "web";

    /// <summary>
    /// The name of the resource the Prologue Extractor runs as.
    /// </summary>
    public const string ExtractorResource = "extractor";

    /// <summary>
    /// The name of the resource the Prologue Receiver runs as.
    /// </summary>
    public const string ReceiverResource = "receiver";

    /// <summary>
    /// The name of the connection string the captures are stored behind.
    /// </summary>
    public const string CaptureStoreConnectionName = "prologue";

    static readonly TimeSpan _startTimeout = TimeSpan.FromMinutes(15);
    static readonly TimeSpan _healthyTimeout = TimeSpan.FromMinutes(10);

    DistributedApplication? _application;
    IPlaywright? _playwright;
    IBrowser? _browser;
    Uri? _proxyAddress;
    Uri? _reactAddress;

    /// <summary>
    /// Gets the Prologue the extractor stamps this composition's captures with.
    /// </summary>
    public static Guid PrologueId => Guid.Parse(LibraryComposition.DefaultPrologueId);

    /// <summary>
    /// Gets why the composition is not running, or <c>null</c> when it is.
    /// </summary>
    public string? UnavailableReason { get; private set; }

    /// <summary>
    /// Gets why no browser can be driven, or <c>null</c> when one can. Kept apart from
    /// <see cref="UnavailableReason"/> so a machine without browsers still runs everything that needs no frontend.
    /// </summary>
    public string? BrowsersUnavailableReason { get; private set; }

    /// <summary>
    /// Gets the store the Receiver persists correlated captures into. Set while the composition starts, so it is
    /// only ever read by a spec the composition is already running for.
    /// </summary>
    public ICaptureStore Captures { get; private set; } = default!;

    /// <inheritdoc/>
    public async Task InitializeAsync()
    {
        UnavailableReason = ContainerRuntime.UnavailableReason;

        if (UnavailableReason is not null)
        {
            return;
        }

        await Start();

        BrowsersUnavailableReason = Browsers.UnavailableReason;

        if (BrowsersUnavailableReason is null)
        {
            _playwright = await Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        }
    }

    /// <inheritdoc/>
    public async Task DisposeAsync()
    {
        if (_browser is not null)
        {
            await _browser.CloseAsync();
        }

        _playwright?.Dispose();

        if (_application is not null)
        {
            await _application.DisposeAsync();
        }
    }

    /// <summary>
    /// Opens a browser on one of the frontends.
    /// </summary>
    /// <param name="frontend">The <see cref="Frontend"/> to drive.</param>
    /// <returns>A <see cref="FrontendDriver"/> on the frontend's entry page.</returns>
    /// <exception cref="RunCannotBeHosted">Thrown when the composition or a browser is missing.</exception>
    public async Task<FrontendDriver> Open(Frontend frontend)
    {
        if (BrowsersUnavailableReason is { } reason)
        {
            throw new RunCannotBeHosted(reason);
        }

        return await FrontendDriver.Open(_browser ?? throw NotRunning(), AddressOf(frontend));
    }

    /// <summary>
    /// Creates a client that talks to the API through the extractor's proxy, which is the only path where a call is
    /// seen and captured.
    /// </summary>
    /// <returns>The <see cref="HttpClient"/> to call the API with.</returns>
    /// <exception cref="RunCannotBeHosted">Thrown when the composition never started.</exception>
    public HttpClient CreateProxyClient() =>
        (_application ?? throw NotRunning()).CreateHttpClient(ExtractorResource, LibraryComposition.ProxyEndpointName);

    /// <summary>
    /// Gets the address a frontend is served from. The Razor frontend is reached through the extractor's proxy —
    /// Core serves it, so its page loads and form posts are captured alongside the API traffic.
    /// </summary>
    /// <param name="frontend">The <see cref="Frontend"/> to get the address of.</param>
    /// <returns>The base address of the frontend.</returns>
    /// <exception cref="RunCannotBeHosted">Thrown when the composition never started.</exception>
    /// <exception cref="UnknownFrontend">Thrown when the composition serves no such frontend.</exception>
    public Uri AddressOf(Frontend frontend) => frontend switch
    {
        Frontend.Razor => _proxyAddress ?? throw NotRunning(),
        Frontend.React => _reactAddress ?? throw NotRunning(),
        _ => throw new UnknownFrontend(frontend)
    };

    async Task Start()
    {
        using var startup = new CancellationTokenSource(_startTimeout);

        var builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Composition>([], startup.Token);

        builder.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Debug);
            logging.AddFilter(nameof(Aspire), LogLevel.Debug);
        });

        builder.Services.ConfigureHttpClientDefaults(client => client.AddStandardResilienceHandler());

        _application = await builder.BuildAsync(startup.Token);
        await _application.StartAsync(startup.Token);

        using var healthy = new CancellationTokenSource(_healthyTimeout);

        // Every browser interaction and every API call in this suite travels through the extractor's proxy, so
        // waiting only for the system behind it is not enough — the extractor starts last, after the Receiver and
        // the database it depends on, and a proxy with nothing behind it accepts connections and then hangs rather
        // than refusing them. Waiting for it is the difference between a clear failure and a wall of timeouts.
        // It carries no health endpoint of its own, so Running is as specific as this can be.
        await _application.ResourceNotifications.WaitForResourceAsync(ReceiverResource, KnownResourceStates.Running, healthy.Token);
        await _application.ResourceNotifications.WaitForResourceAsync(ExtractorResource, KnownResourceStates.Running, healthy.Token);

        await _application.ResourceNotifications.WaitForResourceHealthyAsync(CoreResource, healthy.Token);
        await _application.ResourceNotifications.WaitForResourceHealthyAsync(WebResource, healthy.Token);

        _proxyAddress = _application.GetEndpoint(ExtractorResource, LibraryComposition.ProxyEndpointName);
        _reactAddress = _application.GetEndpoint(WebResource);

        var connectionString = await _application.GetConnectionStringAsync(CaptureStoreConnectionName, startup.Token);

        // The Receiver writes through the same types, so reading with them is reading exactly what it wrote. The
        // database and collection are left at their defaults, which is what the composition configures it with.
        CaptureStorage.RegisterSerializers();
        Captures = new MongoCaptureStore(Options.Create(new PrologueStorageOptions
        {
            Mongo = new MongoOptions { ConnectionString = connectionString! }
        }));
    }

    RunCannotBeHosted NotRunning() =>
        new(UnavailableReason ?? "The distributed library application is not running.");
}
