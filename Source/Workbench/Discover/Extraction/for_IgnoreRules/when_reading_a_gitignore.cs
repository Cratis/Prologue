// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Workbench.Discover.Extraction.for_IgnoreRules;

public class when_reading_a_gitignore : Specification
{
    IgnoreRules _rules;

    void Because() => _rules = IgnoreRules.FromGitignore("""
        # build
        /out-custom/
        scratch
        *.generated.ts
        !keep.cs
        **/deep/**
        """);

    [Fact] void should_skip_a_folder_the_gitignore_names() => _rules.SkipsFolder("scratch").ShouldBeTrue();
    [Fact] void should_skip_a_folder_named_with_slashes() => _rules.SkipsFolder("out-custom").ShouldBeTrue();
    [Fact] void should_skip_the_defaults() => _rules.SkipsFolder("node_modules").ShouldBeTrue();
    [Fact] void should_skip_hidden_folders_but_not_github() => (_rules.SkipsFolder(".cache"), _rules.SkipsFolder(".github")).ShouldEqual((true, false));
    [Fact] void should_skip_an_extension_pattern() => _rules.Reads("client.generated.ts").ShouldBeFalse();
    [Fact] void should_read_other_source() => _rules.Reads("client.ts").ShouldBeTrue();
    [Fact] void should_leave_patterns_it_cannot_read_alone() => _rules.SkipsFolder("deep").ShouldBeFalse();
}
#endif
