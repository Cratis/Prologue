// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Repositories.given;
using Cratis.Prologue.Workbench.Systems;

namespace Cratis.Prologue.Workbench.Repositories.Cloning.for_RepositoryCloner;

public class when_the_clone_fails_with_the_token_in_the_message : a_cloner
{
    CloneResult _result;

    void Establish() => _git.Answer = _ => new GitResult(128, string.Empty, $"fatal: unable to access 'https://x-access-token:{Token}@github.com/cratis/library-platform.git/': The requested URL returned error: 403");

    async Task Because() => _result = await _cloner.Clone(Request("services/core"), CancellationToken.None);

    [Fact] void should_fail() => _result.Succeeded.ShouldBeFalse();
    [Fact] void should_say_why_it_failed() => _result.Failure.Value.ShouldContain("403");
    [Fact] void should_not_expose_the_token() => _result.Failure.Value.ShouldNotContain(Token);
    [Fact] void should_name_the_git_step_that_failed() => _result.Failure.Value.StartsWith("git clone failed (128)", StringComparison.Ordinal).ShouldBeTrue();
}
#endif
