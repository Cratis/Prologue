// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Common;
using Cratis.Prologue.Workbench.Prologues;
using Cratis.Prologue.Workbench.Prologues.given;
using Cratis.Prologue.Workbench.Systems;
using Cratis.Prologue.Workbench.Systems.Locating;

namespace Cratis.Prologue.Workbench.Repositories.Cloning.for_ClonePlan;

public class when_systems_share_a_repository : a_prologue
{
    CloneRequest _request;

    void Because()
    {
        var monorepo = new RepositoryName("cratis/library-platform");
        var main = new BranchName("main");
        var withLending = PrologueDefinition.Reduce(new SystemCodeLocated(_lending, RepositoryHost.GitHub, monorepo, main, new SubPath("services/core")), _prologue);
        var located = PrologueDefinition.Reduce(new SystemCodeLocated(_search, RepositoryHost.GitHub, monorepo, main, new SubPath("services/search")), withLending);
        var connection = new RepositoryConnection(RepositoryConnectionId.For(RepositoryHost.GitHub), RepositoryHost.GitHub, new ServerUrl("https://github.com"), new SecretName("github/prologue-read"), null);

        _request = ClonePlan.For(located, located.Systems.First().Location, connection);
    }

    [Fact] void should_ask_for_the_paths_of_every_system_in_the_repository() => _request.SharedSubPaths.Select(path => path.Value).ShouldContainOnly("services/core", "services/search");
    [Fact] void should_use_the_connections_secret_by_name() => _request.Secret.Value.ShouldEqual("github/prologue-read");
    [Fact] void should_clone_the_systems_own_sub_path() => _request.SubPath.Value.ShouldEqual("services/core");
}
#endif
