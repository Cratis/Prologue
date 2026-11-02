// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Repositories.given;

namespace Cratis.Prologue.Workbench.Repositories.Cloning.for_InstallationTokens;

public class when_one_installation_is_slow : a_github_app
{
    InstallationTokenResult _other;
    Task<InstallationTokenResult> _slow;
    bool _slowFinishedFirst;
    InstallationTokenResult _evicted;
    int _requestsBeforeEviction;
    int _requestsAfterEviction;

    async Task Because()
    {
        var release = new TaskCompletionSource();
        _github.Before = request => request.RequestUri!.AbsolutePath.Contains("/77/", StringComparison.Ordinal) ? release.Task : Task.CompletedTask;
        _slow = _tokens.Mint(_app, _installation, CancellationToken.None);
        _other = await _tokens.Mint(_app, new InstallationId(78), CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(10), TimeProvider.System);
        _slowFinishedFirst = _slow.IsCompleted;
        release.SetResult();
        await _slow;

        _github.Before = null;
        _requestsBeforeEviction = _github.Requests.Count;
        _tokens.Evict(_app, _installation);
        _evicted = await _tokens.Mint(_app, _installation, CancellationToken.None);
        _requestsAfterEviction = _github.Requests.Count;
    }

    [Fact] void should_mint_for_another_installation_meanwhile() => _other.Succeeded.ShouldBeTrue();
    [Fact] void should_not_have_waited_for_the_slow_one() => _slowFinishedFirst.ShouldBeFalse();
    [Fact] void should_ask_github_again_after_the_token_was_evicted() => (_requestsAfterEviction - _requestsBeforeEviction).ShouldEqual(1);
    [Fact] void should_mint_a_token_after_it_was_evicted() => _evicted.Succeeded.ShouldBeTrue();
}
#endif
