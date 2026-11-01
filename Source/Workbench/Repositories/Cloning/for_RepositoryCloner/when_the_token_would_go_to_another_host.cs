// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Common;
using Cratis.Prologue.Workbench.Repositories.given;
using Cratis.Prologue.Workbench.Systems;

namespace Cratis.Prologue.Workbench.Repositories.Cloning.for_RepositoryCloner;

public class when_the_token_would_go_to_another_host : a_cloner
{
    CloneResult _otherHost;
    CloneResult _insecure;
    CloneResult _sameHostWithUserInfo;
    CloneResult _publicElsewhere;

    static CloneRequest Address(string repository, SecretName? secret) =>
        new(RepositoryHost.GitHub, new RepositoryName(repository), new BranchName("main"), new SubPath(string.Empty), [], null, secret, null);

    async Task Because()
    {
        _otherHost = await _cloner.Clone(Address("https://attacker.example/cratis/library-platform.git", new SecretName("github/prologue-read")), CancellationToken.None);
        _insecure = await _cloner.Clone(Address("http://github.com/cratis/library-platform.git", new SecretName("github/prologue-read")), CancellationToken.None);
        _sameHostWithUserInfo = await _cloner.Clone(Address("https://github.com@attacker.example/cratis/library-platform.git", new SecretName("github/prologue-read")), CancellationToken.None);
        _publicElsewhere = await _cloner.Clone(Address("https://git.example.test/public/thing.git", null), CancellationToken.None);
    }

    [Fact] void should_refuse_an_address_on_another_host() => _otherHost.Succeeded.ShouldBeFalse();
    [Fact] void should_say_the_token_is_not_sent_there() => _otherHost.Failure.Value.ShouldContain("not sent");
    [Fact] void should_refuse_an_address_that_is_not_https() => _insecure.Succeeded.ShouldBeFalse();
    [Fact] void should_not_be_fooled_by_a_host_in_the_user_info() => _sameHostWithUserInfo.Succeeded.ShouldBeFalse();
    [Fact] void should_clone_a_public_repository_without_credentials() => _publicElsewhere.Succeeded.ShouldBeTrue();
    [Fact] void should_send_the_token_to_no_one_but_its_host() => _git.Calls.SelectMany(call => call.Environment.Values).Any(value => value.Contains("Authorization", StringComparison.Ordinal)).ShouldBeFalse();
}
#endif
