// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Cratis.Prologue.Workbench.Common;
using Cratis.Prologue.Workbench.Repositories.Cloning;

namespace Cratis.Prologue.Workbench.Repositories.given;

/// <summary>
/// A GitHub App with a private key held as a secret, and a GitHub API that answers through a handler the spec controls.
/// </summary>
public class a_github_app : Specification
{
    protected static readonly RSA _key = RSA.Create(2048);
    protected static readonly string _pem = _key.ExportPkcs8PrivateKeyPem();

    protected static readonly GitHubAppIdentity _app = new(new AppId(4242), new SecretName("github/prologue-app-key"), GitHubAppIdentity.GitHubApi);
    protected static readonly InstallationId _installation = new(77);

    protected FakeGitHub _github;
    protected ManualTime _time;
    protected ISecretResolver _secrets;
    protected InstallationTokens _tokens;

    protected void Establish()
    {
        _github = new FakeGitHub();
        _time = new ManualTime(new DateTimeOffset(2026, 11, 2, 10, 0, 0, TimeSpan.Zero));
        _secrets = Substitute.For<ISecretResolver>();
        _secrets.Resolve(_app.PrivateKey).Returns(_pem);
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient(Arg.Any<string>()).Returns(_ => new HttpClient(_github, disposeHandler: false));
        _tokens = new InstallationTokens(factory, _secrets, _time, Microsoft.Extensions.Logging.Abstractions.NullLogger<InstallationTokens>.Instance);
    }

    /// <summary>
    /// A clock the spec moves.
    /// </summary>
    /// <param name="now">The time it starts at.</param>
    protected class ManualTime(DateTimeOffset now) : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = now;

        public override DateTimeOffset GetUtcNow() => Now;
    }

    /// <summary>
    /// A GitHub API that records the requests it gets and answers with the status and body it was told to.
    /// </summary>
    protected class FakeGitHub : HttpMessageHandler
    {
        public List<HttpRequestMessage> Requests { get; } = [];

        public HttpStatusCode Status { get; set; } = HttpStatusCode.Created;

        public string Token { get; set; } = "ghs_installationtoken1234567890abcdef";

        public DateTimeOffset ExpiresAt { get; set; } = new(2026, 11, 2, 11, 0, 0, TimeSpan.Zero);

        public Exception? Throws { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            if (Throws is not null)
            {
                throw Throws;
            }

            var body = Status == HttpStatusCode.Created
                ? $"{{\"token\":\"{Token}\",\"expires_at\":\"{ExpiresAt:yyyy-MM-ddTHH:mm:ssZ}\"}}"
                : "{\"message\":\"Bad credentials\"}";
            return Task.FromResult(new HttpResponseMessage(Status) { Content = new StringContent(body, Encoding.UTF8, "application/json") });
        }
    }
}
#endif
