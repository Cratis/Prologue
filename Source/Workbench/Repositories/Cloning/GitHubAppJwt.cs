// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Cratis.Prologue.Workbench.Repositories.Cloning;

/// <summary>
/// Builds the token a GitHub App signs in with: an RS256 JWT, valid for nine minutes, that GitHub exchanges for an installation token.
/// </summary>
public static class GitHubAppJwt
{
    /// <summary>
    /// How long before now the token is dated, so a clock that runs ahead of GitHub's still has it accepted.
    /// </summary>
    public static readonly TimeSpan ClockSkew = TimeSpan.FromSeconds(60);

    /// <summary>
    /// How long after now the token expires. GitHub accepts at most ten minutes.
    /// </summary>
    public static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(9);

    /// <summary>
    /// Creates the token.
    /// </summary>
    /// <param name="app">The app, which is the issuer.</param>
    /// <param name="privateKeyPem">The app's private key in PEM form. A key whose line breaks were written as the two characters backslash and n is accepted.</param>
    /// <param name="now">The current time.</param>
    /// <returns>The signed token.</returns>
    /// <exception cref="InvalidPrivateKey">The text is not a private RSA key.</exception>
    public static string Create(AppId app, string privateKeyPem, DateTimeOffset now)
    {
        using var rsa = RSA.Create();
        try
        {
            rsa.ImportFromPem(privateKeyPem.Replace("\\n", "\n", StringComparison.Ordinal));
        }
        catch (Exception exception) when (exception is ArgumentException or CryptographicException)
        {
            // The framework's message can quote the text it failed on, so it is left out.
            throw new InvalidPrivateKey();
        }

        var header = Base64Url(JsonSerializer.SerializeToUtf8Bytes(new { alg = "RS256", typ = "JWT" }));
        var payload = Base64Url(JsonSerializer.SerializeToUtf8Bytes(new
        {
            iat = (now - ClockSkew).ToUnixTimeSeconds(),
            exp = (now + Lifetime).ToUnixTimeSeconds(),
            iss = app.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)
        }));

        var signed = $"{header}.{payload}";
        var signature = rsa.SignData(Encoding.ASCII.GetBytes(signed), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        return $"{signed}.{Base64Url(signature)}";
    }

    static string Base64Url(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
