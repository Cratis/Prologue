// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Discover;

namespace Cratis.Prologue.Workbench.Events.Uncaptured.for_JobMatching;

public class when_matching_a_table_to_a_job : Specification
{
    static readonly CodeJob _noLocation = new("Nightly stock level reconciliation", string.Empty, new SourcePath(string.Empty), new LineNumber(0), new ModuleName(string.Empty));
    static readonly CodeJob _located = new("Nightly stock level sweep", string.Empty, new SourcePath("Jobs/Stock.cs"), new LineNumber(8), new ModuleName("Stock"));
    static readonly CodeJob _detailed = new("Overnight batch", "writes stock_levels and audit_log", new SourcePath(string.Empty), new LineNumber(0), new ModuleName(string.Empty));
    static readonly CodeJob _other = new("Send reminder emails", "reads loans", new SourcePath("Jobs/Mail.cs"), new LineNumber(3), new ModuleName("Mail"));

    [Fact] void should_match_a_job_that_names_the_entity() => JobMatching.For("stock_levels", [_other, _noLocation]).ShouldEqual(_noLocation);
    [Fact] void should_match_a_job_whose_detail_names_the_table() => JobMatching.For("stock_levels", [_other, _detailed]).ShouldEqual(_detailed);
    [Fact] void should_prefer_the_table_as_it_is_over_the_entity() => JobMatching.For("stock_levels", [_located, _detailed]).ShouldEqual(_detailed);
    [Fact] void should_prefer_a_job_with_a_place_in_the_code_when_the_match_is_the_same() => JobMatching.For("StockLevels", [_noLocation, _located]).ShouldEqual(_located);
    [Fact] void should_match_whatever_the_casing_of_the_table() => JobMatching.For("STOCK_LEVELS", [_detailed]).ShouldEqual(_detailed);
    [Fact] void should_not_match_a_job_that_does_not_mention_the_table() => JobMatching.For("stock_levels", [_other]).ShouldBeNull();
    [Fact] void should_not_match_part_of_a_word() => JobMatching.For("loan", [new CodeJob("Reloaned things", string.Empty, new SourcePath("a.cs"), new LineNumber(1), new ModuleName(string.Empty))]).ShouldBeNull();
    [Fact] void should_match_nothing_without_jobs() => JobMatching.For("stock_levels", []).ShouldBeNull();
    [Fact] void should_split_words_on_humps_and_separators() => JobMatching.Words("Stock_LevelsAudit-log").ShouldEqual("stock levels audit log");
}
#endif
