// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Repositories.given;
using Cratis.Prologue.Workbench.Systems;

namespace Cratis.Prologue.Workbench.Repositories.Cloning.for_RepositoryCloner;

public class when_two_systems_share_a_monorepo : a_cloner
{
    CloneResult _core;
    CloneResult _search;

    async Task Because()
    {
        _core = await _cloner.Clone(Request("services/core", shared: "services/search"), CancellationToken.None);
        _search = await _cloner.Clone(Request("services/search", shared: "services/core"), CancellationToken.None);
    }

    [Fact] void should_clone_the_repository_once() => _git.Clones.Count().ShouldEqual(1);
    [Fact] void should_use_the_same_directory() => _core.Directory.ShouldEqual(_search.Directory);
    [Fact] void should_check_out_the_paths_of_both_systems_at_once() => _git.Calls.First(call => call.Arguments[0] == "sparse-checkout").Arguments.ShouldContain("services/core", "services/search");
    [Fact] void should_bring_the_existing_clone_up_to_date_for_the_second_system() => _git.Calls.Count(call => call.Arguments[0] == "fetch").ShouldEqual(1);
}
#endif
