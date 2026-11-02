// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Common;
using Cratis.Prologue.Workbench.Repositories.given;
using Cratis.Prologue.Workbench.Systems;

namespace Cratis.Prologue.Workbench.Repositories.Cloning.for_RepositoryCloner;

public class when_cloning_through_a_github_app : a_cloner
{
    const string InstallationToken = "ghs_installationtoken1234567890abcdef";

    static readonly GitHubAppIdentity _app = new(new AppId(4242), new SecretName("github/prologue-app-key"), GitHubAppIdentity.GitHubApi);

    CloneResult _result;
    CloneResult _otherHost;
    CloneResult _withoutTheApp;
    CloneResult _minting;
    int _mintsForOtherHost;

    static CloneRequest Through(string repository, GitHubAppIdentity? app) =>
        new(RepositoryHost.GitHub, new RepositoryName(repository), new BranchName("main"), new SubPath(string.Empty), [], new ServerUrl("https://github.com"), null, new InstallationId(77), app);

    async Task Because()
    {
        _installationTokens.Mint(_app, new InstallationId(77), Arg.Any<CancellationToken>()).Returns(InstallationTokenResult.Minted(InstallationToken));
        _result = await _cloner.Clone(Through("cratis/library-platform", _app), CancellationToken.None);
        var clonesWithTheToken = _git.Clones.Count();

        _otherHost = await _cloner.Clone(Through("https://attacker.example/cratis/library-platform.git", _app), CancellationToken.None);
        _mintsForOtherHost = _installationTokens.ReceivedCalls().Count();
        _withoutTheApp = await _cloner.Clone(Through("cratis/other", null), CancellationToken.None);

        _installationTokens.Mint(_app, new InstallationId(77), Arg.Any<CancellationToken>()).Returns(InstallationTokenResult.Failed("GitHub rejected the sign-in of app 4242 (401)"));
        _minting = await _cloner.Clone(Through("cratis/failing", _app), CancellationToken.None);
        _clones = clonesWithTheToken;
    }

    int _clones;

    [Fact] void should_succeed() => _result.Succeeded.ShouldBeTrue();
    [Fact] void should_clone_once() => _clones.ShouldEqual(1);
    [Fact] void should_hand_the_installation_token_over_as_a_github_credential() => _git.Clones.First().Environment["GIT_CONFIG_VALUE_0"].ShouldEqual($"Authorization: Basic {GitCredentials.Encode(RepositoryHost.GitHub, InstallationToken)}");
    [Fact] void should_keep_the_token_out_of_every_argument() => _git.Calls.SelectMany(call => call.Arguments).ShouldNotContain(InstallationToken);
    [Fact] void should_not_send_it_to_another_host() => _otherHost.Failure.Value.ShouldContain("not on the server");
    [Fact] void should_not_even_mint_a_token_for_another_host() => _mintsForOtherHost.ShouldEqual(1);
    [Fact] void should_ask_for_a_connection_that_carries_the_app() => _withoutTheApp.Failure.Value.ShouldContain("Connect GitHub again");
    [Fact] void should_report_why_no_token_could_be_minted() => _minting.Failure.Value.ShouldContain("401");
    [Fact] void should_not_clone_when_no_token_could_be_minted() => _git.Clones.Count().ShouldEqual(1);
}
#endif
