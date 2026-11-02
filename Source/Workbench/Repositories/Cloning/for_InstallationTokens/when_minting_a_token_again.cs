// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Repositories.given;

namespace Cratis.Prologue.Workbench.Repositories.Cloning.for_InstallationTokens;

public class when_minting_a_token_again : a_github_app
{
    InstallationTokenResult _first;
    InstallationTokenResult _soonAfter;
    int _requestsAfterReuse;
    InstallationTokenResult _nearExpiry;
    int _requestsAfterRefresh;
    InstallationTokenResult _otherInstallation;
    int _requestsAfterOtherInstallation;

    async Task Because()
    {
        _first = await _tokens.Mint(_app, _installation, CancellationToken.None);
        _time.Now = _time.Now.AddMinutes(30);
        _soonAfter = await _tokens.Mint(_app, _installation, CancellationToken.None);
        _requestsAfterReuse = _github.Requests.Count;

        _github.Token = "ghs_secondtoken1234567890abcdefghij";
        _github.ExpiresAt = _time.Now.AddMinutes(60);
        _time.Now = new DateTimeOffset(2026, 11, 2, 10, 56, 0, TimeSpan.Zero);
        _nearExpiry = await _tokens.Mint(_app, _installation, CancellationToken.None);
        _requestsAfterRefresh = _github.Requests.Count;

        _otherInstallation = await _tokens.Mint(_app, new InstallationId(78), CancellationToken.None);
        _requestsAfterOtherInstallation = _github.Requests.Count;
    }

    [Fact] void should_reuse_a_token_that_is_not_about_to_expire() => _soonAfter.Token.ShouldEqual(_first.Token);
    [Fact] void should_not_ask_github_again_for_it() => _requestsAfterReuse.ShouldEqual(1);
    [Fact] void should_mint_a_new_one_shortly_before_the_old_expires() => _nearExpiry.Token.ShouldEqual("ghs_secondtoken1234567890abcdefghij");
    [Fact] void should_ask_github_for_the_new_one() => _requestsAfterRefresh.ShouldEqual(2);
    [Fact] void should_keep_a_token_per_installation() => _requestsAfterOtherInstallation.ShouldEqual(3);
}
#endif
