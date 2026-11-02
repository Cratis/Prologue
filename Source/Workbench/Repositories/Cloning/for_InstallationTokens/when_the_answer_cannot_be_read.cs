// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Repositories.given;

namespace Cratis.Prologue.Workbench.Repositories.Cloning.for_InstallationTokens;

public class when_the_answer_cannot_be_read : a_github_app
{
    InstallationTokenResult _html;
    InstallationTokenResult _wrongShape;
    InstallationTokenResult _empty;
    InstallationTokenResult _afterwards;

    async Task Because()
    {
        _github.Body = "<html>Bad gateway</html>";
        _html = await _tokens.Mint(_app, _installation, CancellationToken.None);
        _github.Body = "{\"token\":12,\"expires_at\":[]}";
        _wrongShape = await _tokens.Mint(_app, _installation, CancellationToken.None);
        _github.Body = string.Empty;
        _empty = await _tokens.Mint(_app, _installation, CancellationToken.None);
        _github.Body = null;
        _afterwards = await _tokens.Mint(_app, _installation, CancellationToken.None);
    }

    [Fact] void should_fail_for_a_body_that_is_not_json() => _html.Succeeded.ShouldBeFalse();
    [Fact] void should_say_the_body_is_not_a_token() => _html.Failure.ShouldContain("not an installation token");
    [Fact] void should_fail_for_a_token_of_the_wrong_shape() => _wrongShape.Succeeded.ShouldBeFalse();
    [Fact] void should_fail_for_an_empty_body() => _empty.Succeeded.ShouldBeFalse();
    [Fact] void should_not_cache_any_of_it() => _afterwards.Succeeded.ShouldBeTrue();
}
#endif
