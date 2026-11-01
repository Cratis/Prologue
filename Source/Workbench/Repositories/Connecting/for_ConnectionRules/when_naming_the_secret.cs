// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Common;

namespace Cratis.Prologue.Workbench.Repositories.Connecting.for_ConnectionRules;

public class when_naming_the_secret : Specification
{
    ValidationResult _name;
    ValidationResult _blank;
    ValidationResult _gitHubToken;
    ValidationResult _longToken;
    ValidationResult _sentence;

    void Because()
    {
        _name = ConnectionRules.RefuseSecretName(new SecretName("ado/prologue-read"));
        _blank = ConnectionRules.RefuseSecretName(new SecretName(" "));
        _gitHubToken = ConnectionRules.RefuseSecretName(new SecretName("ghp_abcdefghijklmnopqrstuvwxyz0123456789"));
        _longToken = ConnectionRules.RefuseSecretName(new SecretName(new string('a', 52)));
        _sentence = ConnectionRules.RefuseSecretName(new SecretName("my token is abc"));
    }

    [Fact] void should_accept_a_name() => _name.ShouldBeNull();
    [Fact] void should_refuse_a_blank_name() => _blank.ShouldNotBeNull();
    [Fact] void should_refuse_something_that_looks_like_a_github_token() => _gitHubToken.ShouldNotBeNull();
    [Fact] void should_refuse_a_long_run_of_letters_and_digits() => _longToken.ShouldNotBeNull();
    [Fact] void should_refuse_text_with_spaces() => _sentence.ShouldNotBeNull();
    [Fact] void should_explain_that_the_token_is_never_stored() => _gitHubToken.Message.ShouldContain("never stored");
}
#endif
