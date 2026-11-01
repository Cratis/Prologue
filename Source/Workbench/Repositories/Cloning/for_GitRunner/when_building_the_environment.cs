// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Workbench.Repositories.Cloning.for_GitRunner;

public class when_building_the_environment : Specification
{
    [Fact] void should_allow_https_only() => GitRunner.Hardening["GIT_ALLOW_PROTOCOL"].ShouldEqual("https");
    [Fact] void should_never_prompt() => GitRunner.Hardening["GIT_TERMINAL_PROMPT"].ShouldEqual("0");
    [Fact] void should_ignore_the_system_configuration() => GitRunner.Hardening["GIT_CONFIG_NOSYSTEM"].ShouldEqual("1");
    [Fact] void should_ignore_the_user_configuration() => GitRunner.Hardening["GIT_CONFIG_GLOBAL"].ShouldEqual("/dev/null");
}
#endif
