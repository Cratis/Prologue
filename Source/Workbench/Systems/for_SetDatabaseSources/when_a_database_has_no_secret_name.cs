// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Common;
using Cratis.Prologue.Workbench.Common.given;
using Cratis.Prologue.Workbench.Prologues.given;
using Cratis.Prologue.Workbench.Systems.RuntimeSources;

namespace Cratis.Prologue.Workbench.Systems.for_SetDatabaseSources;

public class when_a_database_has_no_secret_name : a_prologue
{
    Result<SystemDatabaseSourcesSet, ValidationResult> _result;

    void Because() => _result = new SetDatabaseSources(_id, _lending, [new DatabaseSource(DatabaseEngine.PostgreSql, new SecretName(" "), [])]).Handle(_prologue);

    [Fact] void should_refuse() => _result.Refusal().ShouldNotBeNull();
}
#endif
