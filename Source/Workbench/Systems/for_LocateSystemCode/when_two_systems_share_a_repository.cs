// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Common.given;
using Cratis.Prologue.Workbench.Prologues;
using Cratis.Prologue.Workbench.Prologues.given;
using Cratis.Prologue.Workbench.Systems.Locating;

namespace Cratis.Prologue.Workbench.Systems.for_LocateSystemCode;

public class when_two_systems_share_a_repository : a_prologue
{
    PrologueDefinition _after;

    void Because()
    {
        var lending = new LocateSystemCode(_id, _lending, RepositoryHost.GitHub, new RepositoryName("cratis/library-platform"), new BranchName("main"), new SubPath("services/core/")).Handle(_prologue).Appended();
        var withLending = PrologueDefinition.Reduce(lending, _prologue);
        var search = new LocateSystemCode(_id, _search, RepositoryHost.GitHub, new RepositoryName("cratis/library-platform"), new BranchName("main"), new SubPath("services/search")).Handle(withLending).Appended();
        _after = PrologueDefinition.Reduce(search, withLending);
    }

    [Fact] void should_point_the_first_system_at_its_sub_path() => _after.Systems.Single(system => system.Id == _lending).Location.SubPath.Value.ShouldEqual("services/core");
    [Fact] void should_point_the_second_system_at_its_own_sub_path() => _after.Systems.Single(system => system.Id == _search).Location.SubPath.Value.ShouldEqual("services/search");
    [Fact] void should_share_the_repository() => _after.Systems.Select(system => system.Location.Repository.Value).Distinct().Count().ShouldEqual(1);
}
#endif
