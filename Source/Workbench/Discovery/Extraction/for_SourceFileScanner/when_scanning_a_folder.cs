// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Discovery.given;

namespace Cratis.Prologue.Workbench.Discovery.Extraction.for_SourceFileScanner;

public class when_scanning_a_folder : a_source_folder
{
    IReadOnlyList<SourceFile> _files;
    IReadOnlyList<SourceFile> _missing;

    void Because()
    {
        _files = SourceFileScanner.Scan(_root);
        _missing = SourceFileScanner.Scan(Path.Combine(_root, "nowhere"));
    }

    [Fact] void should_find_the_source_files_in_path_order() => _files.Select(_ => _.Path.Value).ShouldEqual(["Loans/LoanPolicy.cs", "Loans/LoansController.cs", "Program.cs"]);
    [Fact] void should_skip_build_output_dependencies_and_git() => _files.Any(_ => _.Path.Value.Contains("bin/") || _.Path.Value.Contains("obj/") || _.Path.Value.Contains("node_modules") || _.Path.Value.Contains(".git")).ShouldBeFalse();
    [Fact] void should_skip_generated_files() => _files.Any(_ => _.Path.Value.EndsWith(".g.cs", StringComparison.Ordinal)).ShouldBeFalse();
    [Fact] void should_skip_files_that_are_not_code() => _files.Any(_ => _.Path.Value.EndsWith(".md", StringComparison.Ordinal)).ShouldBeFalse();
    [Fact] void should_skip_files_over_the_size_cap() => _files.Any(_ => _.Path.Value.Contains("Huge", StringComparison.Ordinal)).ShouldBeFalse();
    [Fact] void should_find_nothing_in_a_folder_that_does_not_exist() => _missing.ShouldBeEmpty();
}
#endif
