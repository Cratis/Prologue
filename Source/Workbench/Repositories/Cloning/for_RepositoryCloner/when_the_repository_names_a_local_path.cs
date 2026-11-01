// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Common;
using Cratis.Prologue.Workbench.Repositories.given;
using Cratis.Prologue.Workbench.Systems;

namespace Cratis.Prologue.Workbench.Repositories.Cloning.for_RepositoryCloner;

public class when_the_repository_names_a_local_path : a_cloner
{
    static readonly string[] _values = ["file:///etc", "../../etc", "~/secrets", "C:\\repos\\x", "ext::sh -c id", "/srv/legacy\\x"];

    List<CloneResult> _results;

    async Task Because()
    {
        _results = [];
        foreach (var value in _values)
        {
            _results.Add(await _cloner.Clone(Request(string.Empty, RepositoryHost.GitHub, value), CancellationToken.None));
        }
    }

    [Fact] void should_refuse_every_one() => _results.TrueForAll(result => !result.Succeeded).ShouldBeTrue();
    [Fact] void should_not_run_git() => _git.Calls.ShouldBeEmpty();
}
#endif
