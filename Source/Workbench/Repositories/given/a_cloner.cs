// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Common;
using Cratis.Prologue.Workbench.Repositories.Cloning;
using Cratis.Prologue.Workbench.Systems;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Cratis.Prologue.Workbench.Repositories.given;

/// <summary>
/// A cloner over a recording git runner, a secret that resolves to a token and a temporary clone directory.
/// </summary>
public class a_cloner : Specification, IDisposable
{
    protected const string Token = "ghp_supersecrettoken1234567890";

    protected RecordingGitRunner _git;
    protected ISecretResolver _secrets;
    protected RepositoryCloner _cloner;
    protected string _root;

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }

        GC.SuppressFinalize(this);
    }

    protected static CloneRequest Request(string subPath, RepositoryHost host = RepositoryHost.GitHub, string repository = "cratis/library-platform", params string[] shared) =>
        new(
            host,
            new RepositoryName(repository),
            new BranchName("main"),
            new SubPath(subPath),
            shared.Select(path => new SubPath(path)),
            host == RepositoryHost.AzureDevOps ? new ServerUrl("https://dev.azure.com/contoso") : null,
            new SecretName("github/prologue-read"),
            null);

    void Establish()
    {
        _root = Path.Combine(Path.GetTempPath(), $"prologue-clones-{Guid.NewGuid():N}");
        _git = new RecordingGitRunner();
        _secrets = Substitute.For<ISecretResolver>();
        _secrets.Resolve(Arg.Any<SecretName>()).Returns(Token);
        _cloner = new RepositoryCloner(_git, _secrets, Options.Create(new RepositoryOptions { CloneDirectory = _root }), Microsoft.Extensions.Logging.Abstractions.NullLogger<RepositoryCloner>.Instance);
    }
}
#endif
