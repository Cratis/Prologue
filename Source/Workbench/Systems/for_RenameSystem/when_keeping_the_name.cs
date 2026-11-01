// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Common.given;
using Cratis.Prologue.Workbench.Prologues.given;
using Cratis.Prologue.Workbench.Systems.Renaming;

namespace Cratis.Prologue.Workbench.Systems.for_RenameSystem;

public class when_keeping_the_name : a_prologue
{
    Result<SystemRenamed, ValidationResult> _result;

    void Because() => _result = new RenameSystem(_id, _search, new ServiceName("Search")).Handle(_prologue);

    [Fact] void should_not_record_a_rename_that_changes_nothing() => _result.Appended().ShouldBeNull();
}
#endif
