// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Prologues;
using Cratis.Prologue.Workbench.Prologues.given;
using Cratis.Prologue.Workbench.Systems;
using Cratis.Prologue.Workbench.Systems.Locating;

namespace Cratis.Prologue.Workbench.Discovery.Analyzing.for_AnalysisTargets.given;

/// <summary>
/// The Library prologue with Lending in a monorepo on GitHub and Search in a local folder.
/// </summary>
public class a_located_prologue : a_prologue
{
    protected PrologueDefinition _located;

    void Establish()
    {
        var lending = PrologueDefinition.Reduce(
            new SystemCodeLocated(_lending, RepositoryHost.GitHub, new RepositoryName("cratis/library-platform"), new BranchName("main"), new SubPath("services/core")),
            _prologue)!;
        _located = PrologueDefinition.Reduce(
            new SystemCodeLocated(_search, RepositoryHost.LocalFolder, new RepositoryName("/srv/library"), new BranchName("main"), new SubPath("search")),
            lending)!;
    }
}
#endif
