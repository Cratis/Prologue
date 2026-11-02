// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using Cratis.Prologue.Workbench.Repositories.given;

namespace Cratis.Prologue.Workbench.Repositories.Cloning.for_InstallationTokens;

public class when_minting_fails : a_github_app
{
    InstallationTokenResult _unauthorized;
    InstallationTokenResult _forbidden;
    InstallationTokenResult _notFound;
    InstallationTokenResult _serverError;
    InstallationTokenResult _unreachable;
    InstallationTokenResult _noSecret;
    InstallationTokenResult _notAKey;
    InstallationTokenResult _notHttps;
    InstallationTokenResult _afterwards;

    async Task Because()
    {
        _github.Status = HttpStatusCode.Unauthorized;
        _unauthorized = await _tokens.Mint(_app, _installation, CancellationToken.None);
        _github.Status = HttpStatusCode.Forbidden;
        _forbidden = await _tokens.Mint(_app, _installation, CancellationToken.None);
        _github.Status = HttpStatusCode.NotFound;
        _notFound = await _tokens.Mint(_app, _installation, CancellationToken.None);
        _github.Status = HttpStatusCode.InternalServerError;
        _serverError = await _tokens.Mint(_app, _installation, CancellationToken.None);

        _github.Throws = new HttpRequestException($"connection refused. Authorization: Bearer secret {_pem}");
        _unreachable = await _tokens.Mint(_app, _installation, CancellationToken.None);
        _github.Throws = null;
        var requestsBefore = _github.Requests.Count;

        _secrets.Resolve(_app.PrivateKey).Returns((string?)null);
        _noSecret = await _tokens.Mint(_app, _installation, CancellationToken.None);
        _secrets.Resolve(_app.PrivateKey).Returns("not a key");
        _notAKey = await _tokens.Mint(_app, _installation, CancellationToken.None);
        _secrets.Resolve(_app.PrivateKey).Returns(_pem);
        _notHttps = await _tokens.Mint(_app with { Api = new ServerUrl("http://api.github.com") }, _installation, CancellationToken.None);

        _github.Status = HttpStatusCode.Created;
        _afterwards = await _tokens.Mint(_app, _installation, CancellationToken.None);
        _requestsWithoutGitHub = _github.Requests.Count - requestsBefore;
    }

    int _requestsWithoutGitHub;

    [Fact] void should_say_the_sign_in_was_rejected() => _unauthorized.Failure.ShouldContain("401");
    [Fact] void should_point_at_the_app_id_and_the_key() => _unauthorized.Failure.ShouldContain("github/prologue-app-key");
    [Fact] void should_say_github_refused_the_installation() => _forbidden.Failure.ShouldContain("403");
    [Fact] void should_say_the_installation_was_not_found() => _notFound.Failure.ShouldContain("installation 77");
    [Fact] void should_say_which_status_github_answered_with() => _serverError.Failure.ShouldContain("500");
    [Fact] void should_say_github_could_not_be_reached() => _unreachable.Failure.ShouldContain("api.github.com");
    [Fact] void should_not_give_a_token_for_any_failure() => new[] { _unauthorized, _forbidden, _notFound, _serverError, _unreachable, _noSecret, _notAKey, _notHttps }.All(result => result.Token is null).ShouldBeTrue();
    [Fact] void should_name_the_missing_secret() => _noSecret.Failure.ShouldContain("'github/prologue-app-key'");
    [Fact] void should_say_the_secret_is_not_a_key() => _notAKey.Failure.ShouldContain("PEM");
    [Fact] void should_not_quote_a_value_that_is_not_a_key() => _notAKey.Failure.ShouldNotContain("not a key");
    [Fact] void should_not_sign_in_over_http() => _notHttps.Failure.ShouldContain("https");
    [Fact] void should_not_reach_github_without_a_usable_key_or_https() => _requestsWithoutGitHub.ShouldEqual(1);
    [Fact] void should_not_cache_a_failure() => _afterwards.Succeeded.ShouldBeTrue();
    [Fact] void should_keep_the_key_out_of_every_failure() => new[] { _unauthorized, _forbidden, _notFound, _serverError, _unreachable, _noSecret, _notAKey, _notHttps }.Any(result => result.Failure.Contains("PRIVATE KEY", StringComparison.Ordinal)).ShouldBeFalse();
    [Fact] void should_keep_the_signed_token_out_of_every_failure() => new[] { _unauthorized, _forbidden, _notFound, _serverError, _unreachable }.Any(result => result.Failure.Contains("eyJ", StringComparison.Ordinal)).ShouldBeFalse();
}
#endif
