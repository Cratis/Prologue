// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Cratis.Prologue.Workbench.Repositories.given;

namespace Cratis.Prologue.Workbench.Repositories.Cloning.for_InstallationTokens;

public class when_minting_a_token : a_github_app
{
    InstallationTokenResult _result;
    HttpRequestMessage _request;
    string _bearer;
    bool _signedByTheApp;

    async Task Because()
    {
        _result = await _tokens.Mint(_app, _installation, CancellationToken.None);
        _request = _github.Requests.Single();
        _bearer = _request.Headers.Authorization!.Parameter!;
        var parts = _bearer.Split('.');
        var signature = Convert.FromBase64String(parts[2].Replace('-', '+').Replace('_', '/').PadRight((parts[2].Length + 3) / 4 * 4, '='));
        _signedByTheApp = _key.VerifyData(Encoding.ASCII.GetBytes($"{parts[0]}.{parts[1]}"), signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    }

    [Fact] void should_succeed() => _result.Succeeded.ShouldBeTrue();
    [Fact] void should_return_the_installation_token() => _result.Token.ShouldEqual(_github.Token);
    [Fact] void should_post() => _request.Method.ShouldEqual(HttpMethod.Post);
    [Fact] void should_ask_for_the_installation() => _request.RequestUri!.AbsoluteUri.ShouldEqual("https://api.github.com/app/installations/77/access_tokens");
    [Fact] void should_sign_in_as_the_app_with_a_bearer_token() => _request.Headers.Authorization!.Scheme.ShouldEqual("Bearer");
    [Fact] void should_sign_with_the_key_of_the_app() => _signedByTheApp.ShouldBeTrue();
    [Fact] void should_ask_for_the_github_json_media_type() => _request.Headers.Accept.ShouldContain(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
    [Fact] void should_name_the_api_version() => _request.Headers.GetValues("X-GitHub-Api-Version").Single().ShouldEqual("2022-11-28");
    [Fact] void should_identify_itself() => _request.Headers.UserAgent.ToString().ShouldContain("Cratis-Prologue");
}
#endif
