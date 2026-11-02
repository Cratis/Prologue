// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using Cratis.DependencyInjection;
using Cratis.Prologue.Workbench.Systems;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Prologue.Workbench.Repositories.Cloning;

/// <summary>
/// Clones repositories with the git command line: shallow, one branch, only the paths the systems live in. Systems in
/// the same repository and branch share one clone. Access tokens reach git only through its environment.
/// </summary>
/// <param name="git">Runs git.</param>
/// <param name="secrets">Resolves the access token by its name.</param>
/// <param name="installationTokens">Mints the installation token a GitHub App connection clones with.</param>
/// <param name="options">The configured clone directory.</param>
/// <param name="logger">The logger.</param>
[Singleton]
public class RepositoryCloner(IGitRunner git, ISecretResolver secrets, IInstallationTokens installationTokens, IOptions<RepositoryOptions> options, ILogger<RepositoryCloner> logger) : IRepositoryCloner
{
    readonly ConcurrentDictionary<string, SemaphoreSlim> _gates = new(StringComparer.Ordinal);

    /// <inheritdoc/>
    public async Task<CloneResult> Clone(CloneRequest request, CancellationToken cancellationToken)
    {
        if (Refuse(request) is { } problem)
        {
            return CloneResult.Failed(problem);
        }

        if (request.Host == RepositoryHost.LocalFolder)
        {
            return await Local(request, cancellationToken);
        }

        var url = CloneUrl.For(request);
        if (url is null)
        {
            return CloneResult.Failed($"Connect {request.Host} before cloning from it");
        }

        if (CloneUrl.Refusal(request, url) is { } refusal)
        {
            return CloneResult.Failed(refusal);
        }

        var (token, failure) = await Authenticate(request, url, cancellationToken);
        if (failure is not null)
        {
            return CloneResult.Failed(failure);
        }

        var directory = CloneDirectories.For(options.Value.CloneDirectory, request);
        var gate = _gates.GetOrAdd(directory, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken);
        try
        {
            return await Synchronize(request, url, directory, token, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            var reason = Redact(exception.Message, request, token);
            logger.CloneFailed(request.Repository.Value, reason);
            return CloneResult.Failed(reason);
        }
        finally
        {
            gate.Release();
        }
    }

    static string? Refuse(CloneRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Repository.Value))
        {
            return "The system has no repository";
        }

        if (new[] { request.SubPath }.Concat(request.SharedSubPaths).Any(SparsePaths.EscapesRepository))
        {
            return "A sub path has to stay inside the repository";
        }

        return request.Branch.Value.StartsWith('-') || request.Repository.Value.TrimStart().StartsWith('-')
            ? "The repository and branch cannot start with a dash"
            : null;
    }

    static string Redact(string text, CloneRequest request, string? token) =>
        SecretRedaction.Redact(text, token is null ? [] : [token, GitCredentials.Encode(request.Host, token)]);

    static int CountFiles(string directory) =>
        Directory.Exists(directory)
            ? Directory.EnumerateFiles(directory, "*", new EnumerationOptions { RecurseSubdirectories = true, AttributesToSkip = FileAttributes.System })
                .Count(file => !file.Contains($"{Path.DirectorySeparatorChar}.git{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            : 0;

    static string Describe(CloneStep step, GitResult result, CloneRequest request, string? token)
    {
        var lines = result.Error.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).TakeLast(3);
        var text = $"git {step.Arguments[0]} failed ({result.ExitCode}): {string.Join(' ', lines)}";
        var redacted = Redact(text, request, token);
        return redacted.Length > 500 ? redacted[..500] : redacted;
    }

    async Task<(string? Token, string? Failure)> Authenticate(CloneRequest request, string url, CancellationToken cancellationToken)
    {
        if (request.Installation is { } installation)
        {
            if (request.App is not { } app)
            {
                return (null, "The GitHub App connection was recorded without the app's id and private key. Connect GitHub again with them");
            }

            if (!CloneUrl.MayCarryToken(request, url))
            {
                return (null, $"The repository address is not on the server of the {request.Host} connection, so no installation token is minted for it");
            }

            var minted = await installationTokens.Mint(app, installation, cancellationToken);
            return minted.Succeeded ? (minted.Token, null) : (null, minted.Failure);
        }

        if (request.Secret is not { } secret)
        {
            return (null, null);
        }

        if (!CloneUrl.MayCarryToken(request, url))
        {
            return (null, $"The repository address is not on the server of the {request.Host} connection, so its access token is not sent there");
        }

        var token = secrets.Resolve(secret);
        return token is null ? (null, $"The secret '{secret.Value}' is not configured on the Receiver") : (token, null);
    }

    async Task<CloneResult> Local(CloneRequest request, CancellationToken cancellationToken)
    {
        var root = request.Repository.Value.Trim();
        var path = Path.Combine(root, SparsePaths.Normalize(request.SubPath));
        if (!Directory.Exists(path))
        {
            return CloneResult.Failed($"The folder '{path}' does not exist on the Receiver");
        }

        var head = await git.Run(["rev-parse", "HEAD"], root, ReadOnlyDictionary<string, string>.Empty, cancellationToken);
        return CloneResult.Cloned(head.Succeeded ? head.Output.Trim() : "working tree", CountFiles(path), root);
    }

    async Task<CloneResult> Synchronize(CloneRequest request, string url, string directory, string? token, CancellationToken cancellationToken)
    {
        var environment = GitCredentials.Environment(request.Host, token);
        var sparse = SparsePaths.For([.. request.SharedSubPaths, request.SubPath]);
        var branch = request.Branch.Value.Trim();

        var steps = Directory.Exists(Path.Combine(directory, ".git"))
            ? CloneSteps.Update(branch, sparse)
            : CloneSteps.Clone(url, directory, branch, sparse);

        if (!Directory.Exists(Path.Combine(directory, ".git")))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(directory)!);
        }

        foreach (var step in steps)
        {
            var result = await git.Run(step.Arguments, step.InDirectory ? directory : null, environment, cancellationToken);
            if (!result.Succeeded)
            {
                return CloneResult.Failed(Describe(step, result, request, token));
            }
        }

        var head = await git.Run(["rev-parse", "HEAD"], directory, environment, cancellationToken);
        if (!head.Succeeded)
        {
            return CloneResult.Failed(Describe(new CloneStep(["rev-parse"], true), head, request, token));
        }

        return CloneResult.Cloned(head.Output.Trim(), CountFiles(Path.Combine(directory, SparsePaths.Normalize(request.SubPath))), directory);
    }
}
