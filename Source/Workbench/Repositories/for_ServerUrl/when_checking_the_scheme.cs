// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Workbench.Repositories.for_ServerUrl;

public class when_checking_the_scheme : Specification
{
    [Fact] void should_accept_https() => new ServerUrl("https://git.contoso.example").IsValid.ShouldBeTrue();
    [Fact] void should_accept_http() => new ServerUrl("http://git.contoso.example").IsValid.ShouldBeTrue();
    [Fact] void should_refuse_a_file_address() => new ServerUrl("file:///srv/host").IsValid.ShouldBeFalse();
    [Fact] void should_refuse_something_that_is_not_an_address() => new ServerUrl("/srv/host").IsValid.ShouldBeFalse();
}
#endif
