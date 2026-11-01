// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Systems;

namespace Cratis.Prologue.Workbench.Repositories.Cloning.for_SparsePaths;

public class when_a_sub_path_leaves_the_repository : Specification
{
    bool _parent;
    bool _nested;
    bool _inside;

    void Because()
    {
        _parent = SparsePaths.EscapesRepository(new SubPath("../secrets"));
        _nested = SparsePaths.EscapesRepository(new SubPath("services/../../etc"));
        _inside = SparsePaths.EscapesRepository(new SubPath("services/core"));
    }

    [Fact] void should_refuse_a_parent_path() => _parent.ShouldBeTrue();
    [Fact] void should_refuse_a_nested_parent_path() => _nested.ShouldBeTrue();
    [Fact] void should_accept_a_path_inside() => _inside.ShouldBeFalse();
}
#endif
