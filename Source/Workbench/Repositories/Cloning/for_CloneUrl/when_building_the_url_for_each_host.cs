// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Common;
using Cratis.Prologue.Workbench.Systems;

namespace Cratis.Prologue.Workbench.Repositories.Cloning.for_CloneUrl;

public class when_building_the_url_for_each_host : Specification
{
    static CloneRequest Request(RepositoryHost host, string repository, string? server = null, SecretName? secret = null) =>
        new(host, new RepositoryName(repository), new BranchName("main"), new SubPath(string.Empty), [], server is null ? null : new ServerUrl(server), secret, null);

    string _gitHub;
    string _gitHubEnterprise;
    string _gitLab;
    string _selfHostedGitLab;
    string _azureDevOps;
    string _azureDevOpsSameName;
    string _azureDevOpsWithoutConnection;
    string _local;
    string _withCredentialsInTheUrl;

    void Because()
    {
        _gitHub = CloneUrl.For(Request(RepositoryHost.GitHub, "cratis/library-platform", "https://github.com", new SecretName("github/token")));
        _gitHubEnterprise = CloneUrl.For(Request(RepositoryHost.GitHub, "/cratis/library-platform.git/", "https://git.contoso.example/"));
        _gitLab = CloneUrl.For(Request(RepositoryHost.GitLab, "library/notifications"));
        _selfHostedGitLab = CloneUrl.For(Request(RepositoryHost.GitLab, "library/notifications", "https://gitlab.contoso.example"));
        _azureDevOps = CloneUrl.For(Request(RepositoryHost.AzureDevOps, "library/library-ops", "https://dev.azure.com/contoso"));
        _azureDevOpsSameName = CloneUrl.For(Request(RepositoryHost.AzureDevOps, "library-ops", "https://dev.azure.com/contoso"));
        _azureDevOpsWithoutConnection = CloneUrl.For(Request(RepositoryHost.AzureDevOps, "library/library-ops"));
        _local = CloneUrl.For(Request(RepositoryHost.LocalFolder, "/srv/legacy"));
        _withCredentialsInTheUrl = CloneUrl.For(Request(RepositoryHost.GitHub, "https://user:hunter2@github.com/cratis/library-platform.git"));
    }

    [Fact] void should_build_the_github_url() => _gitHub.ShouldEqual("https://github.com/cratis/library-platform.git");
    [Fact] void should_build_a_github_enterprise_url_from_the_server() => _gitHubEnterprise.ShouldEqual("https://git.contoso.example/cratis/library-platform.git");
    [Fact] void should_default_gitlab_to_gitlab_com() => _gitLab.ShouldEqual("https://gitlab.com/library/notifications.git");
    [Fact] void should_build_a_self_hosted_gitlab_url() => _selfHostedGitLab.ShouldEqual("https://gitlab.contoso.example/library/notifications.git");
    [Fact] void should_build_an_azure_devops_url_from_project_and_repository() => _azureDevOps.ShouldEqual("https://dev.azure.com/contoso/library/_git/library-ops");
    [Fact] void should_use_the_name_as_project_when_azure_devops_has_only_a_repository() => _azureDevOpsSameName.ShouldEqual("https://dev.azure.com/contoso/library-ops/_git/library-ops");
    [Fact] void should_need_a_connection_for_azure_devops() => _azureDevOpsWithoutConnection.ShouldBeNull();
    [Fact] void should_use_a_local_folder_as_it_is() => _local.ShouldEqual("/srv/legacy");
    [Fact] void should_never_carry_credentials() => _withCredentialsInTheUrl.ShouldNotContain("hunter2");
}
#endif
