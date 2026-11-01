// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Common;
using Cratis.Prologue.Workbench.Systems;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Cratis.Prologue.Workbench.Repositories.Cloning.for_RepositoryCloner;

/// <summary>
/// Runs the real git command line against a repository on disk standing in for a host.
/// </summary>
public class when_cloning_a_monorepo_with_git : Specification, IDisposable
{
    static readonly Dictionary<string, string> _author = new()
    {
        ["GIT_AUTHOR_NAME"] = "Prologue",
        ["GIT_AUTHOR_EMAIL"] = "prologue@example.com",
        ["GIT_COMMITTER_NAME"] = "Prologue",
        ["GIT_COMMITTER_EMAIL"] = "prologue@example.com"
    };

    readonly GitRunner _git = new();

    string _directory;
    string _source;
    string _firstCommit;
    string _secondCommit;
    CloneRequest _core;
    CloneRequest _search;
    CloneResult _first;
    CloneResult _sharing;
    CloneResult _updated;

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            foreach (var file in Directory.EnumerateFiles(_directory, "*", SearchOption.AllDirectories))
            {
                File.SetAttributes(file, FileAttributes.Normal);
            }

            Directory.Delete(_directory, recursive: true);
        }

        GC.SuppressFinalize(this);
    }

    async Task Because()
    {
        _directory = Path.Combine(Path.GetTempPath(), $"prologue-git-{Guid.NewGuid():N}");
        _source = Path.Combine(_directory, "host", "cratis", "library-platform.git");
        Directory.CreateDirectory(_source);

        await Git(_source, "init", "--quiet", "--initial-branch=main");
        await Git(_source, "config", "uploadpack.allowFilter", "true");
        await Git(_source, "config", "uploadpack.allowAnySHA1InWant", "true");
        Write("services/core/Program.cs");
        Write("services/core/Loans.cs");
        Write("services/search/Index.cs");
        Write("services/billing/Invoices.cs");
        _firstCommit = await Commit("first");

        var server = new ServerUrl(new Uri(Path.Combine(_directory, "host")).AbsoluteUri.TrimEnd('/'));
        var cloner = new RepositoryCloner(_git, new NoSecrets(), Options.Create(new RepositoryOptions { CloneDirectory = Path.Combine(_directory, "clones") }), Microsoft.Extensions.Logging.Abstractions.NullLogger<RepositoryCloner>.Instance);

        _core = Request(server, "services/core", "services/search");
        _search = Request(server, "services/search", "services/core");

        _first = await cloner.Clone(_core, CancellationToken.None);
        _sharing = await cloner.Clone(_search, CancellationToken.None);

        Write("services/core/Returns.cs");
        _secondCommit = await Commit("second");
        _updated = await cloner.Clone(_core, CancellationToken.None);
    }

    static CloneRequest Request(ServerUrl server, string subPath, string sibling) =>
        new(RepositoryHost.GitHub, new RepositoryName("cratis/library-platform"), new BranchName("main"), new SubPath(subPath), [new SubPath(sibling)], server, null, null);

    [Fact] void should_clone_the_first_time() => (_first.Failure?.Value ?? string.Empty).ShouldEqual(string.Empty);
    [Fact] void should_be_at_the_commit_of_the_branch() => _first.Commit.Value.ShouldEqual(_firstCommit);
    [Fact] void should_count_the_files_under_the_sub_path() => _first.Files.Value.ShouldEqual(2);
    [Fact] void should_check_out_the_sub_path() => File.Exists(Path.Combine(_first.Directory, "services", "core", "Program.cs")).ShouldBeTrue();
    [Fact] void should_check_out_the_path_of_the_other_system_too() => File.Exists(Path.Combine(_first.Directory, "services", "search", "Index.cs")).ShouldBeTrue();
    [Fact] void should_leave_out_paths_no_system_lives_in() => Directory.Exists(Path.Combine(_first.Directory, "services", "billing")).ShouldBeFalse();
    [Fact] void should_share_the_clone_between_the_systems() => _sharing.Directory.ShouldEqual(_first.Directory);
    [Fact] void should_count_the_files_of_the_second_system() => _sharing.Files.Value.ShouldEqual(1);
    [Fact] void should_pull_the_new_commit_when_asked_again() => _updated.Commit.Value.ShouldEqual(_secondCommit);
    [Fact] void should_count_the_new_file() => _updated.Files.Value.ShouldEqual(3);

    async Task Git(string workingDirectory, params string[] arguments)
    {
        var result = await _git.Run(arguments, workingDirectory, _author, CancellationToken.None);
        result.Error.Trim().ShouldNotContain("fatal");
        result.ExitCode.ShouldEqual(0);
    }

    async Task<string> Commit(string message)
    {
        await Git(_source, "add", "--all");
        await Git(_source, "commit", "--quiet", "-m", message);
        return (await _git.Run(["rev-parse", "HEAD"], _source, _author, CancellationToken.None)).Output.Trim();
    }

    void Write(string relativePath)
    {
        var path = Path.Combine(_source, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, "// code");
    }

    sealed class NoSecrets : ISecretResolver
    {
        public string? Resolve(SecretName name) => null;
    }
}
#endif
