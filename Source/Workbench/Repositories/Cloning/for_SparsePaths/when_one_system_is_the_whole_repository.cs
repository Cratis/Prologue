// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Systems;

namespace Cratis.Prologue.Workbench.Repositories.Cloning.for_SparsePaths;

public class when_one_system_is_the_whole_repository : Specification
{
    IReadOnlyList<string> _result;

    void Because() => _result = SparsePaths.For([new SubPath("services/core"), new SubPath(string.Empty)]);

    [Fact] void should_not_narrow_the_checkout() => _result.ShouldBeEmpty();
}
#endif
