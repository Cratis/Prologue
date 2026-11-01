// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Common.given;
using Cratis.Prologue.Workbench.Prologues.given;
using Cratis.Prologue.Workbench.Systems.Adding;

namespace Cratis.Prologue.Workbench.Systems.for_AddSystem;

public class when_the_name_is_free : a_prologue
{
    SystemAdded _event;

    void Because() => _event = new AddSystem(_id, SystemId.New(), new ServiceName("  Fines ")).Handle(_prologue).Appended();

    [Fact] void should_add_the_system() => _event.ShouldNotBeNull();
    [Fact] void should_use_the_trimmed_name_as_the_service_name() => _event.Name.Value.ShouldEqual("Fines");
}
#endif
