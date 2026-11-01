// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Systems;

namespace Cratis.Prologue.Workbench.Repositories.Cloning.for_CloneDirectories;

public class when_systems_share_a_repository : Specification
{
    static CloneRequest Request(string subPath, string branch = "main", string repository = "cratis/library-platform") =>
        new(RepositoryHost.GitHub, new RepositoryName(repository), new BranchName(branch), new SubPath(subPath), [], null, null, null);

    string _core;
    string _search;
    string _otherBranch;
    string _hostile;

    void Because()
    {
        _core = CloneDirectories.For("/var/prologue/repos", Request("services/core"));
        _search = CloneDirectories.For("/var/prologue/repos", Request("services/search"));
        _otherBranch = CloneDirectories.For("/var/prologue/repos", Request("services/core", "release/1.0"));
        _hostile = CloneDirectories.For("/var/prologue/repos", Request("x", "main", "../../etc/passwd"));
    }

    [Fact] void should_share_one_directory() => _core.ShouldEqual(_search);
    [Fact] void should_keep_a_branch_apart() => _otherBranch.ShouldNotEqual(_core);
    [Fact] void should_keep_the_branch_out_of_the_path_structure() => _otherBranch.EndsWith("release_1.0", StringComparison.Ordinal).ShouldBeTrue();
    [Fact] void should_stay_inside_the_clone_directory() => _hostile.StartsWith("/var/prologue/repos/github/", StringComparison.Ordinal).ShouldBeTrue();
    [Fact] void should_not_climb_out_through_dots() => _hostile.ShouldNotContain("..");
}
#endif
