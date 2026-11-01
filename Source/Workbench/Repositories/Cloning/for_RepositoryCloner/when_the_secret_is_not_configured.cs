// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Common;
using Cratis.Prologue.Workbench.Repositories.given;

namespace Cratis.Prologue.Workbench.Repositories.Cloning.for_RepositoryCloner;

public class when_the_secret_is_not_configured : a_cloner
{
    CloneResult _result;

    void Establish() => _secrets.Resolve(Arg.Any<SecretName>()).Returns((string)null);

    async Task Because() => _result = await _cloner.Clone(Request("services/core"), CancellationToken.None);

    [Fact] void should_fail_naming_the_secret() => _result.Failure.Value.ShouldContain("github/prologue-read");
    [Fact] void should_not_run_git() => _git.Calls.ShouldBeEmpty();
}
#endif
