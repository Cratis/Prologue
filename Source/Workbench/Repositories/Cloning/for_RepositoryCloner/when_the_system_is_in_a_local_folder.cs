// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Common;
using Cratis.Prologue.Workbench.Repositories.given;
using Cratis.Prologue.Workbench.Systems;

namespace Cratis.Prologue.Workbench.Repositories.Cloning.for_RepositoryCloner;

public class when_the_system_is_in_a_local_folder : a_cloner
{
    string _folder;
    CloneResult _result;

    void Establish()
    {
        _folder = Path.Combine(Path.GetTempPath(), $"prologue-local-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Path.Combine(_folder, "services", "core"));
        File.WriteAllText(Path.Combine(_folder, "services", "core", "Program.cs"), "// code");
        File.WriteAllText(Path.Combine(_folder, "services", "core", "Loans.cs"), "// code");
        File.WriteAllText(Path.Combine(_folder, "README.md"), "# not under the sub path");
    }

    async Task Because() => _result = await _cloner.Clone(
        new CloneRequest(RepositoryHost.LocalFolder, new RepositoryName(_folder), new BranchName("main"), new SubPath("services/core"), [], null, null, null),
        CancellationToken.None);

    void Destroy() => Directory.Delete(_folder, recursive: true);

    [Fact] void should_succeed_without_a_connection() => _result.Succeeded.ShouldBeTrue();
    [Fact] void should_count_the_files_under_the_sub_path() => _result.Files.Value.ShouldEqual(2);
    [Fact] void should_not_clone_anything() => _git.Clones.ShouldBeEmpty();
    [Fact] void should_not_resolve_a_secret() => _secrets.DidNotReceive().Resolve(Arg.Any<SecretName>());
}
#endif
