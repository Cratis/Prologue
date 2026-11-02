// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Common;
using Cratis.Prologue.Workbench.Repositories.given;
using Cratis.Prologue.Workbench.Systems;

namespace Cratis.Prologue.Workbench.Repositories.Cloning.for_RepositoryCloner;

public class when_the_installation_token_goes_wrong : a_cloner
{
    static readonly GitHubAppIdentity _app = new(new AppId(4242), new SecretName("github/prologue-app-key"), GitHubAppIdentity.GitHubApi);

    CloneResult _throwing;
    CloneResult _rejected;
    CloneResult _otherFailure;
    int _evictionsAfterRejection;

    static CloneRequest Through(string repository) =>
        new(RepositoryHost.GitHub, new RepositoryName(repository), new BranchName("main"), new SubPath(string.Empty), [], new ServerUrl("https://github.com"), null, new InstallationId(77), _app);

    async Task Because()
    {
        _installationTokens.Mint(_app, new InstallationId(77), Arg.Any<CancellationToken>()).Returns<Task<InstallationTokenResult>>(_ => throw new InvalidOperationException("the key is broken"));
        _throwing = await _cloner.Clone(Through("cratis/one"), CancellationToken.None);

        _installationTokens.Mint(_app, new InstallationId(77), Arg.Any<CancellationToken>()).Returns(InstallationTokenResult.Minted("ghs_installationtoken1234567890abcdef"));
        _git.Answer = arguments => arguments[0] == "clone" ? new GitResult(128, string.Empty, "remote: Invalid username or token.\nfatal: Authentication failed for 'https://github.com/cratis/two.git/'") : new GitResult(0, string.Empty, string.Empty);
        _rejected = await _cloner.Clone(Through("cratis/two"), CancellationToken.None);
        _evictionsAfterRejection = _installationTokens.ReceivedCalls().Count(call => call.GetMethodInfo().Name == nameof(IInstallationTokens.Evict));

        _git.Answer = arguments => arguments[0] == "clone" ? new GitResult(128, string.Empty, "fatal: repository not found") : new GitResult(0, string.Empty, string.Empty);
        _otherFailure = await _cloner.Clone(Through("cratis/three"), CancellationToken.None);
    }

    [Fact] void should_end_a_credential_that_throws_as_a_failed_clone() => _throwing.Succeeded.ShouldBeFalse();
    [Fact] void should_say_getting_the_credential_failed() => _throwing.Failure.Value.ShouldContain("Getting the credential");
    [Fact] void should_not_repeat_the_exception_message() => _throwing.Failure.Value.ShouldNotContain("the key is broken");
    [Fact] void should_fail_the_clone_the_server_rejected() => _rejected.Succeeded.ShouldBeFalse();
    [Fact] void should_evict_the_token_the_server_rejected() => _evictionsAfterRejection.ShouldEqual(1);
    [Fact] void should_keep_the_token_when_the_failure_is_something_else() => _installationTokens.ReceivedCalls().Count(call => call.GetMethodInfo().Name == nameof(IInstallationTokens.Evict)).ShouldEqual(1);
    [Fact] void should_still_fail_that_clone() => _otherFailure.Succeeded.ShouldBeFalse();
}
#endif
