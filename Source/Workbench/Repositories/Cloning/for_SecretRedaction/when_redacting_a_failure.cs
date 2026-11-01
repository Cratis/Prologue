// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Workbench.Repositories.Cloning.for_SecretRedaction;

public class when_redacting_a_failure : Specification
{
    const string Token = "glpat-abcdef123456";

    string _result;

    void Because() => _result = SecretRedaction.Redact(
        $"fatal: unable to access 'https://oauth2:{Token}@gitlab.com/library/notifications.git/': 403. Authorization: Basic b2F1dGgyOmdscGF0LWFiY2RlZjEyMzQ1Ng== sent {Token}",
        [Token]);

    [Fact] void should_remove_the_token() => _result.ShouldNotContain(Token);
    [Fact] void should_remove_credentials_from_an_address() => _result.ShouldNotContain("oauth2:");
    [Fact] void should_remove_the_authorization_header_value() => _result.ShouldNotContain("b2F1dGgy");
    [Fact] void should_keep_the_rest_of_the_message() => _result.ShouldContain("gitlab.com/library/notifications.git");
}
#endif
