// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Systems;

namespace Cratis.Prologue.Workbench.Repositories.Cloning.for_SparsePaths;

public class when_systems_share_a_monorepo : Specification
{
    IReadOnlyList<string> _result;

    void Because() => _result = SparsePaths.For([new SubPath("services/search/"), new SubPath("\\services\\core"), new SubPath("services/core"), new SubPath("/jobs/stocktake")]);

    [Fact] void should_check_out_each_path_once() => _result.Count.ShouldEqual(3);
    [Fact] void should_normalize_them() => _result.ShouldContainOnly("jobs/stocktake", "services/core", "services/search");
}
#endif
