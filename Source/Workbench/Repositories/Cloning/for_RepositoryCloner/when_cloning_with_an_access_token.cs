// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Repositories.given;
using Cratis.Prologue.Workbench.Systems;

namespace Cratis.Prologue.Workbench.Repositories.Cloning.for_RepositoryCloner;

public class when_cloning_with_an_access_token : a_cloner
{
    CloneResult _result;

    async Task Because() => _result = await _cloner.Clone(Request("services/core"), CancellationToken.None);

    [Fact] void should_succeed() => _result.Succeeded.ShouldBeTrue();
    [Fact] void should_report_the_commit() => _result.Commit.Value.ShouldEqual("abc123");
    [Fact] void should_clone_shallow_and_only_the_branch() => _git.Clones.Single().Arguments.ShouldContain("--depth=1", "--single-branch", "--branch=main");
    [Fact] void should_clone_sparse_when_a_sub_path_is_set() => _git.Clones.Single().Arguments.ShouldContain("--sparse");
    [Fact] void should_check_out_only_the_sub_path() => _git.Calls.Single(call => call.Arguments[0] == "sparse-checkout").Arguments.ShouldContain("services/core");
    [Fact] void should_clone_from_an_address_without_credentials() => _git.Clones.Single().Arguments.ShouldContain("https://github.com/cratis/library-platform.git");
    [Fact] void should_hand_the_token_over_only_in_the_environment() => _git.Clones.Single().Environment["GIT_CONFIG_VALUE_0"].StartsWith("Authorization: Basic ", StringComparison.Ordinal).ShouldBeTrue();
    [Fact] void should_keep_the_token_out_of_every_argument() => _git.Calls.SelectMany(call => call.Arguments).ShouldNotContain(Token);
    [Fact] void should_keep_the_token_out_of_the_result() => (_result.Directory + _result.Commit.Value).ShouldNotContain(Token);
}
#endif
