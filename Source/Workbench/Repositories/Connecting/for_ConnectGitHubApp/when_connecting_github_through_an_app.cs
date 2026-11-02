// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Common;
using Cratis.Prologue.Workbench.Common.given;
using Cratis.Prologue.Workbench.Systems;

namespace Cratis.Prologue.Workbench.Repositories.Connecting.for_ConnectGitHubApp;

public class when_connecting_github_through_an_app : Specification
{
    static readonly ConnectGitHubApp _command = new(new ServerUrl("https://github.com"), new InstallationId(77), new AppId(4242), new SecretName("github/prologue-app-key"), new ServerUrl(string.Empty));

    GitHubAppInstallationConnected _event;
    GitHubAppInstallationConnected _enterprise;
    RepositoryConnection _connection;
    RepositoryConnection _replayed;
    Result<GitHubAppInstallationConnected, ValidationResult> _again;
    Result<GitHubAppInstallationConnected, ValidationResult> _insecureApi;
    Result<GitHubAppInstallationConnected, ValidationResult> _foreignApi;
    Result<GitHubAppInstallationConnected, ValidationResult> _keyItself;
    Result<GitHubAppInstallationConnected, ValidationResult> _noApp;

    void Because()
    {
        var context = EventContext.EmptyWithEventSourceId(RepositoryConnectionId.For(RepositoryHost.GitHub));
        _event = _command.Handle(null).Appended();
        _connection = RepositoryConnection.Reduce(_event, null, context);
        _again = _command.Handle(_connection);
        _enterprise = (_command with { Url = new ServerUrl("https://ghe.example.com"), Api = new ServerUrl("https://ghe.example.com/api/v3/") }).Handle(null).Appended();
        _insecureApi = (_command with { Api = new ServerUrl("http://api.github.com") }).Handle(null);
        _foreignApi = (_command with { Api = new ServerUrl("https://attacker.example") }).Handle(null);
        _keyItself = (_command with { PrivateKey = new SecretName("-----BEGIN PRIVATE KEY----- abc") }).Handle(null);
        _noApp = (_command with { App = new AppId(0) }).Handle(null);

        // A connection recorded before the app's id and private key were asked for must still replay.
        _replayed = RepositoryConnection.Reduce(new GitHubAppConnected(new ServerUrl("https://github.com"), new InstallationId(77)), null, context);
    }

    [Fact] void should_default_to_the_github_api() => _event.App.Api.Value.ShouldEqual("https://api.github.com");
    [Fact] void should_record_the_app_and_the_installation() => (_event.App.App.Value, _event.Installation.Value).ShouldEqual((4242L, 77L));
    [Fact] void should_record_only_the_name_of_the_private_key() => _event.App.PrivateKey.Value.ShouldEqual("github/prologue-app-key");
    [Fact] void should_keep_the_app_on_the_connection() => _connection.App.ShouldEqual(_event.App);
    [Fact] void should_not_record_the_same_connection_twice() => _again.Appended().ShouldBeNull();
    [Fact] void should_accept_the_api_of_a_github_enterprise_server() => _enterprise.App.Api.Value.ShouldEqual("https://ghe.example.com/api/v3");
    [Fact] void should_refuse_an_api_that_is_not_https() => _insecureApi.Refusal().ShouldNotBeNull();
    [Fact] void should_refuse_an_api_on_another_host() => _foreignApi.Refusal().ShouldNotBeNull();
    [Fact] void should_refuse_a_private_key_given_as_the_secret_name() => _keyItself.Refusal().ShouldNotBeNull();
    [Fact] void should_refuse_a_missing_app_id() => _noApp.Refusal().ShouldNotBeNull();
    [Fact] void should_replay_a_connection_recorded_without_the_app() => (_replayed.Installation.Value, _replayed.App).ShouldEqual((77L, null));
}
#endif
