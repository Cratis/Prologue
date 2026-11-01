// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Common.given;
using Cratis.Prologue.Workbench.Prologues.given;
using Cratis.Prologue.Workbench.Systems.Renaming;

namespace Cratis.Prologue.Workbench.Systems.for_RenameSystem;

public class when_renaming_to_another_systems_name : a_prologue
{
    Result<SystemRenamed, ValidationResult> _result;

    void Because() => _result = new RenameSystem(_id, _search, new ServiceName("Lending")).Handle(_prologue);

    [Fact] void should_refuse() => _result.Refusal().ShouldNotBeNull();
}
#endif
