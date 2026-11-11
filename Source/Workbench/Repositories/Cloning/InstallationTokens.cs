// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Cratis.Prologue.Workbench.Repositories.Cloning;

/// <summary>
/// Represents an <see cref="IInstallationTokens"/> that signs in as the app with its private key, resolved from a secret by name, and
/// asks the GitHub API for an installation access token. Nothing here is persisted or logged: a token is cached in memory until
/// shortly before it expires, and a failure is described without the key, the signed token or the access token.
/// </summary>
/// <param name="http">Creates the client requests are made with.</param>
/// <param name="secrets">Resolves the secret holding the private key.</param>
/// <param name="time">The clock.</param>
/// <param name="logger">The logger.</param>
public class InstallationTokens(IHttpClientFactory http, ISecretResolver secrets, TimeProvider time, ILogger<InstallationTokens> logger) : IInstallationTokens, IDisposable
{
    /// <summary>
    /// How long before a token expires it stops being reused, so a clone that starts with it can finish.
    /// </summary>
    public static readonly TimeSpan RefreshMargin = TimeSpan.FromMinutes(5);

    readonly ConcurrentDictionary<string, CachedToken> _cache = new(StringComparer.Ordinal);
    readonly ConcurrentDictionary<string, SemaphoreSlim> _gates = new(StringComparer.Ordinal);

    /// <inheritdoc/>
    public async Task<InstallationTokenResult> Mint(GitHubAppIdentity app, InstallationId installation, CancellationToken cancellationToken)
    {
        var key = KeyOf(app, installation);
        EvictExpired();
        if (Cached(key) is { } cached)
        {
            return InstallationTokenResult.Minted(cached);
        }

        // One gate per installation, so a slow GitHub for one installation does not hold up every other clone.
        var gate = _gates.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken);
        try
        {
            return Cached(key) is { } minted ? InstallationTokenResult.Minted(minted) : await Request(key, app, installation, cancellationToken);
        }
        finally
        {
            gate.Release();
        }
    }

    /// <inheritdoc/>
    public void Evict(GitHubAppIdentity app, InstallationId installation) => _cache.TryRemove(KeyOf(app, installation), out _);

    /// <inheritdoc/>
    public void Dispose()
    {
        foreach (var gate in _gates.Values)
        {
            gate.Dispose();
        }

        GC.SuppressFinalize(this);
    }

    static string KeyOf(GitHubAppIdentity app, InstallationId installation) => $"{app.Api.Value}|{app.App.Value}|{installation.Value}|{app.PrivateKey.Value}";

    static string Describe(HttpStatusCode status, GitHubAppIdentity app, InstallationId installation, Uri address) => status switch
    {
        HttpStatusCode.Unauthorized => $"GitHub rejected the sign-in of app {app.App.Value} (401). Check the app id and that the secret '{app.PrivateKey.Value}' holds one of the app's current private keys",
        HttpStatusCode.Forbidden => $"GitHub refused to give app {app.App.Value} a token for installation {installation.Value} (403). The installation may be suspended or the app lacks the permission to read repository contents",
        HttpStatusCode.NotFound => $"GitHub found no installation {installation.Value} for app {app.App.Value} at {address.Host} (404). Check the installation id and the API address",
        _ => $"GitHub answered {(int)status} when asked for a token for installation {installation.Value}"
    };

    static (string Value, DateTimeOffset ExpiresAt)? ReadToken(JsonElement root)
    {
        if (!root.TryGetProperty("token", out var token) || token.GetString() is not { Length: > 0 } value)
        {
            return null;
        }

        return root.TryGetProperty("expires_at", out var expires) && DateTimeOffset.TryParse(expires.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var expiresAt)
            ? (value, expiresAt)
            : null;
    }

    void EvictExpired()
    {
        var now = time.GetUtcNow();
        foreach (var entry in _cache.Where(entry => entry.Value.ExpiresAt <= now))
        {
            _cache.TryRemove(entry);
        }
    }

    string? Cached(string key) =>
        _cache.TryGetValue(key, out var cached) && cached.ExpiresAt - RefreshMargin > time.GetUtcNow() ? cached.Token : null;

    async Task<InstallationTokenResult> Request(string key, GitHubAppIdentity app, InstallationId installation, CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(app.Api.Value.TrimEnd('/') + $"/app/installations/{installation.Value}/access_tokens", UriKind.Absolute, out var address) || address.Scheme != Uri.UriSchemeHttps)
        {
            return InstallationTokenResult.Failed("The GitHub API address has to start with https://; the app is not signed in anywhere else");
        }

        var pem = secrets.Resolve(app.PrivateKey);
        if (pem is null)
        {
            return InstallationTokenResult.Failed($"The secret '{app.PrivateKey.Value}' holding the GitHub App's private key is not configured on the Receiver");
        }

        string jwt;
        try
        {
            jwt = GitHubAppJwt.Create(app.App, pem, time.GetUtcNow());
        }
        catch (InvalidPrivateKey exception)
        {
            return InstallationTokenResult.Failed($"The secret '{app.PrivateKey.Value}' does not hold a PEM-encoded RSA private key: {exception.Message}");
        }

        try
        {
            return await Exchange(new(key, address, app, installation, [pem, jwt]), jwt, cancellationToken);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
        {
            var failure = SecretRedaction.Redact($"GitHub could not be reached at {address.Host}: {exception.GetType().Name}", [pem, jwt]);
            logger.TokenRequestFailed(app.App.Value, failure);
            return InstallationTokenResult.Failed(failure);
        }
    }

    async Task<InstallationTokenResult> Exchange(TokenExchange exchange, string jwt, CancellationToken cancellationToken)
    {
        var (key, address, app, installation, secretsInPlay) = exchange;
        using var request = new HttpRequestMessage(HttpMethod.Post, address);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        request.Headers.UserAgent.Add(new ProductInfoHeaderValue("Cratis-Prologue", "1"));
        request.Headers.Add("X-GitHub-Api-Version", "2022-11-28");

        using var client = http.CreateClient();
        using var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var failure = SecretRedaction.Redact(Describe(response.StatusCode, app, installation, address), secretsInPlay);
            logger.TokenRequestFailed(app.App.Value, failure);
            return InstallationTokenResult.Failed(failure);
        }

        await using var body = await response.Content.ReadAsStreamAsync(cancellationToken);
        try
        {
            using var document = await JsonDocument.ParseAsync(body, cancellationToken: cancellationToken);
            if (ReadToken(document.RootElement) is not var (value, expiresAt))
            {
                return InstallationTokenResult.Failed("GitHub answered without an installation token");
            }

            _cache[key] = new CachedToken(value, expiresAt);
            return InstallationTokenResult.Minted(value);
        }
        catch (Exception exception) when (exception is JsonException or InvalidOperationException)
        {
            // A 200 whose body is not the JSON object GitHub documents (a proxy's page, a changed shape): a failure to report, never an exception out of Mint.
            var failure = SecretRedaction.Redact($"GitHub answered with a body that is not an installation token ({exception.GetType().Name})", secretsInPlay);
            logger.TokenRequestFailed(app.App.Value, failure);
            return InstallationTokenResult.Failed(failure);
        }
    }

    readonly record struct TokenExchange(string Key, Uri Address, GitHubAppIdentity App, InstallationId Installation, string[] SecretsInPlay);

    sealed record CachedToken(string Token, DateTimeOffset ExpiresAt);
}
