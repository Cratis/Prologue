// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Common.given;
using Cratis.Prologue.Workbench.Prologues;
using Cratis.Prologue.Workbench.Prologues.given;
using Cratis.Prologue.Workbench.Systems.Locating;

namespace Cratis.Prologue.Workbench.Systems.for_LocateSystemCode;

public class when_locating_the_code_where_it_already_is : a_prologue
{
    Result<SystemCodeLocated, ValidationResult> _result;

    void Because()
    {
        var command = new LocateSystemCode(_id, _lending, RepositoryHost.GitLab, new RepositoryName("library/notifications"), new BranchName("master"), new SubPath(string.Empty));
        var located = PrologueDefinition.Reduce(command.Handle(_prologue).Appended(), _prologue);
        _result = command.Handle(located);
    }

    [Fact] void should_not_record_the_same_location_twice() => _result.Appended().ShouldBeNull();
}
#endif
