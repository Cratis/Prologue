// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Library.Core.Pages;

/// <summary>
/// Wires up the Razor server-rendered frontend — the client its page handlers reach the library API through.
/// </summary>
public static class RazorFrontend
{
    const string ApiBaseAddressKey = "Library:Frontend:ApiBaseAddress";
    const string ServerUrlsKey = "ASPNETCORE_URLS";
    const string DefaultApiBaseAddress = "http://localhost:5080";

    /// <summary>
    /// Registers the typed <see cref="LibraryApi"/> the Razor page handlers call, along with the named
    /// <see cref="HttpClient"/> it uses.
    /// </summary>
    /// <param name="builder">The <see cref="IHostApplicationBuilder"/> to add registrations to.</param>
    /// <returns>The <paramref name="builder"/> for chaining.</returns>
    /// <remarks>
    /// The client points at the system itself, not at the Prologue Extractor's reverse proxy. For a
    /// server-rendered frontend the HTTP worth capturing is the browser's form POST to the page — routing the
    /// page handler's own call back through the proxy would only duplicate it.
    /// </remarks>
    public static IHostApplicationBuilder AddRazorFrontend(this IHostApplicationBuilder builder)
    {
        var baseAddress = ResolveBaseAddress(builder.Configuration);

        builder.Services.AddHttpClient(LibraryApi.HttpClientName, client =>
        {
            client.BaseAddress = baseAddress;
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        builder.Services.AddSingleton<ApiTransport>();
        builder.Services.AddSingleton<LibraryApi>();

        return builder;
    }

    static Uri ResolveBaseAddress(IConfiguration configuration)
    {
        var configured = configuration[ApiBaseAddressKey];

        if (string.IsNullOrWhiteSpace(configured))
        {
            configured = FirstServerUrl(configuration) ?? DefaultApiBaseAddress;
        }

        return new Uri(configured.TrimEnd('/') + "/");
    }

    static string? FirstServerUrl(IConfiguration configuration)
    {
        var urls = configuration[ServerUrlsKey]?.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [];
        var url = urls.FirstOrDefault(candidate => candidate.StartsWith("http://", StringComparison.OrdinalIgnoreCase)) ?? urls.FirstOrDefault();

        // A wildcard is what the server binds to, not somewhere a client can dial.
        return url?.Replace("+", "localhost", StringComparison.Ordinal).Replace("*", "localhost", StringComparison.Ordinal);
    }
}
