// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Common;
using Cratis.Prologue.Workbench.Common.given;
using Cratis.Prologue.Workbench.Systems;

namespace Cratis.Prologue.Workbench.Repositories.Connecting.for_ConnectGitHub;

public class when_connecting_github : Specification
{
    static readonly ConnectGitHub _command = new(new ServerUrl("https://github.com"), new SecretName("github/prologue-read"));

    GitHubConnected _event;
    Result<GitHubConnected, ValidationResult> _again;
    Result<GitHubConnected, ValidationResult> _badAddress;

    void Because()
    {
        _event = _command.Handle(null).Appended();
        var current = RepositoryConnection.Reduce(_event, null, EventContext.EmptyWithEventSourceId(RepositoryConnectionId.For(RepositoryHost.GitHub)));
        _again = _command.Handle(current);
        _badAddress = new ConnectGitHub(new ServerUrl("github"), new SecretName("github/prologue-read")).Handle(null);
    }

    [Fact] void should_record_the_name_of_the_secret() => _event.Secret.Value.ShouldEqual("github/prologue-read");
    [Fact] void should_append_to_the_stream_of_the_host() => _command.GetEventSourceId().Value.ShouldEqual("GitHub");
    [Fact] void should_not_record_the_same_connection_twice() => _again.Appended().ShouldBeNull();
    [Fact] void should_refuse_an_address_that_is_not_a_server() => _badAddress.Refusal().ShouldNotBeNull();
}
#endif
