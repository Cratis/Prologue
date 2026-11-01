// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Common.given;
using Cratis.Prologue.Workbench.Prologues;
using Cratis.Prologue.Workbench.Systems.Adding;

namespace Cratis.Prologue.Workbench.Systems.for_AddSystem;

public class when_the_prologue_is_not_defined : Specification
{
    Result<SystemAdded, ValidationResult> _result;

    void Because() => _result = new AddSystem(PrologueId.New(), SystemId.New(), new ServiceName("Fines")).Handle(null);

    [Fact] void should_refuse() => _result.Refusal().ShouldNotBeNull();
}
#endif
