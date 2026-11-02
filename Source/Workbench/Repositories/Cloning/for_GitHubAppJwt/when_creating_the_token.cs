// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Cratis.Prologue.Workbench.Repositories.given;

namespace Cratis.Prologue.Workbench.Repositories.Cloning.for_GitHubAppJwt;

public class when_creating_the_token : a_github_app
{
    static readonly DateTimeOffset Now = new(2026, 11, 2, 10, 0, 0, TimeSpan.Zero);

    string _jwt;
    string _withEscapedLineBreaks;
    string[] _parts;
    JsonElement _header;
    JsonElement _claims;
    bool _signatureIsValid;
    InvalidPrivateKey? _invalid;

    void Because()
    {
        _jwt = GitHubAppJwt.Create(_app.App, _pem, Now);
        _withEscapedLineBreaks = GitHubAppJwt.Create(_app.App, _pem.Replace("\n", "\\n", StringComparison.Ordinal), Now);
        _parts = _jwt.Split('.');
        _header = JsonDocument.Parse(Decode(_parts[0])).RootElement;
        _claims = JsonDocument.Parse(Decode(_parts[1])).RootElement;
        _signatureIsValid = _key.VerifyData(Encoding.ASCII.GetBytes($"{_parts[0]}.{_parts[1]}"), DecodeBytes(_parts[2]), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        try
        {
            GitHubAppJwt.Create(_app.App, "-----BEGIN PRIVATE KEY-----\nnot-a-key\n-----END PRIVATE KEY-----", Now);
        }
        catch (InvalidPrivateKey exception)
        {
            _invalid = exception;
        }
    }

    static byte[] DecodeBytes(string part) => Convert.FromBase64String(part.Replace('-', '+').Replace('_', '/').PadRight((part.Length + 3) / 4 * 4, '='));

    static string Decode(string part) => Encoding.UTF8.GetString(DecodeBytes(part));

    [Fact] void should_have_three_parts() => _parts.Length.ShouldEqual(3);
    [Fact] void should_sign_with_rs256() => _header.GetProperty("alg").GetString().ShouldEqual("RS256");
    [Fact] void should_be_a_jwt() => _header.GetProperty("typ").GetString().ShouldEqual("JWT");
    [Fact] void should_name_the_app_as_issuer() => _claims.GetProperty("iss").GetString().ShouldEqual("4242");
    [Fact] void should_be_issued_a_minute_ago() => _claims.GetProperty("iat").GetInt64().ShouldEqual(Now.AddSeconds(-60).ToUnixTimeSeconds());
    [Fact] void should_expire_in_nine_minutes() => _claims.GetProperty("exp").GetInt64().ShouldEqual(Now.AddMinutes(9).ToUnixTimeSeconds());
    [Fact] void should_carry_a_signature_the_public_key_verifies() => _signatureIsValid.ShouldBeTrue();
    [Fact] void should_not_use_base64_padding_or_url_unsafe_characters() => (_jwt.Contains('=') || _jwt.Contains('+') || _jwt.Contains('/')).ShouldBeFalse();
    [Fact] void should_accept_a_key_whose_line_breaks_were_escaped() => _withEscapedLineBreaks.ShouldEqual(_jwt);
    [Fact] void should_refuse_text_that_is_not_a_key() => _invalid.ShouldNotBeNull();
    [Fact] void should_not_quote_the_key_in_the_refusal() => _invalid!.Message.ShouldNotContain("not-a-key");
}
#endif
