// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Common.given;
using Cratis.Prologue.Workbench.Prologues;
using Cratis.Prologue.Workbench.Prologues.given;
using Cratis.Prologue.Workbench.Systems.Renaming;

namespace Cratis.Prologue.Workbench.Systems.for_RenameSystem;

public class when_renaming_to_a_free_name : a_prologue
{
    SystemRenamed _event;
    PrologueDefinition _after;

    void Because()
    {
        _event = new RenameSystem(_id, _search, new ServiceName("Catalog search")).Handle(_prologue).Appended();
        _after = PrologueDefinition.Reduce(_event, _prologue);
    }

    [Fact] void should_record_the_rename() => _event.Name.Value.ShouldEqual("Catalog search");
    [Fact] void should_change_the_service_name() => _after.Systems.Single(system => system.Id == _search).Name.Value.ShouldEqual("Catalog search");
    [Fact] void should_leave_the_other_system_alone() => _after.Systems.Single(system => system.Id == _lending).Name.Value.ShouldEqual("Lending");
}
#endif
