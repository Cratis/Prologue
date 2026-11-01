// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Microsoft.Extensions.Options;

namespace Cratis.Prologue.Workbench.Discover.Running.for_SourceFolders;

public class when_checking_the_allowed_roots : Specification
{
    static SourceFolders With(params string[] roots) => new(Options.Create(new DiscoverOptions { AllowedRoots = [.. roots] }));

    SourceFolders _unrestricted;
    SourceFolders _restricted;

    void Establish()
    {
        _unrestricted = With();
        _restricted = With("/srv/legacy", "/srv/other/");
    }

    [Fact] void should_allow_any_folder_when_no_roots_are_configured() => _unrestricted.IsAllowed("/etc").ShouldBeTrue();
    [Fact] void should_allow_a_root_itself() => _restricted.IsAllowed("/srv/legacy").ShouldBeTrue();
    [Fact] void should_allow_a_folder_inside_a_root() => _restricted.IsAllowed("/srv/legacy/library/src").ShouldBeTrue();
    [Fact] void should_allow_a_folder_inside_a_root_given_with_a_trailing_slash() => _restricted.IsAllowed("/srv/other/app").ShouldBeTrue();
    [Fact] void should_refuse_a_folder_outside_every_root() => _restricted.IsAllowed("/etc").ShouldBeFalse();
    [Fact] void should_refuse_a_sibling_that_only_shares_the_prefix() => _restricted.IsAllowed("/srv/legacy-secrets").ShouldBeFalse();
    [Fact] void should_refuse_a_path_that_climbs_out_with_dots() => _restricted.IsAllowed("/srv/legacy/../../etc").ShouldBeFalse();
    [Fact] void should_allow_a_path_that_climbs_and_comes_back() => _restricted.IsAllowed("/srv/other/../legacy/library").ShouldBeTrue();
    [Fact] void should_ignore_blank_roots() => With(string.Empty, "  ").IsAllowed("/etc").ShouldBeTrue();
}
#endif
