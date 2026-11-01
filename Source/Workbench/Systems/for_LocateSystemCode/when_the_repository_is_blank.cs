// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Common.given;
using Cratis.Prologue.Workbench.Prologues.given;
using Cratis.Prologue.Workbench.Systems.Locating;

namespace Cratis.Prologue.Workbench.Systems.for_LocateSystemCode;

public class when_the_repository_is_blank : a_prologue
{
    Result<SystemCodeLocated, ValidationResult> _result;

    void Because() => _result = new LocateSystemCode(_id, _lending, RepositoryHost.LocalFolder, new RepositoryName(" "), new BranchName("main"), new SubPath(string.Empty)).Handle(_prologue);

    [Fact] void should_ask_which_folder() => _result.Refusal().Message.ShouldContain("folder");
}
#endif
