// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Common;
using Cratis.Prologue.Workbench.Repositories.Connecting;
using Cratis.Prologue.Workbench.Systems;

namespace Cratis.Prologue.Workbench.Repositories.for_RepositoryConnection;

public class when_a_host_is_connected_in_different_ways_and_disconnected : Specification
{
    RepositoryConnection _withToken;
    RepositoryConnection _withApp;
    RepositoryConnection _afterDisconnect;

    void Because()
    {
        var context = EventContext.EmptyWithEventSourceId(RepositoryConnectionId.For(RepositoryHost.GitHub));
        _withToken = RepositoryConnection.Reduce(new GitHubConnected(new ServerUrl("https://github.com"), new SecretName("github/prologue-read")), null, context);
        _withApp = RepositoryConnection.Reduce(new GitHubAppConnected(new ServerUrl("https://github.com"), new InstallationId(42)), _withToken, context);
        _afterDisconnect = RepositoryConnection.Reduce(new RepositoryHostDisconnected(RepositoryHost.GitHub), _withApp);
    }

    [Fact] void should_keep_the_name_of_the_secret_and_nothing_else_of_the_token() => _withToken.Secret.Value.ShouldEqual("github/prologue-read");
    [Fact] void should_belong_to_the_host() => _withToken.Host.ShouldEqual(RepositoryHost.GitHub);
    [Fact] void should_drop_the_secret_when_the_app_replaces_the_token() => _withApp.Secret.ShouldBeNull();
    [Fact] void should_keep_the_installation() => _withApp.Installation.Value.ShouldEqual(42);
    [Fact] void should_have_no_connection_after_disconnecting() => _afterDisconnect.ShouldBeNull();
}
#endif
