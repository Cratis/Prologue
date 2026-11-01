// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Workbench.Discovery.Agents.for_AnalysisOutputParser;

public class when_parsing_dependencies : Specification
{
    const string Answer = """
        ```
        {"dependencies":[
          {"target":"Notifications","kind":"http","detail":"POST /notify"},
          {"target":"library-db","kind":"DB","detail":"writes loans"},
          {"target":"Stock-take","kind":"schedule"},
          {"target":"","kind":"http"},
          {"target":"Mystery","kind":"telepathy"}
        ]}
        ```
        """;

    ParsedFindings<FoundDependency> _parsed;

    void Because() => _parsed = AnalysisOutputParser.Dependencies(Answer);

    [Fact] void should_keep_the_valid_dependencies() => _parsed.Items.Count.ShouldEqual(3);
    [Fact] void should_read_the_kind_in_any_case_and_by_alias() => _parsed.Items.Select(_ => _.Kind).ShouldContainOnly(DependencyKind.Http, DependencyKind.Database, DependencyKind.Job);
    [Fact] void should_keep_the_detail() => _parsed.Items[0].Detail.Value.ShouldEqual("POST /notify");
    [Fact] void should_accept_a_missing_detail() => _parsed.Items[2].Detail.Value.ShouldEqual(string.Empty);
    [Fact] void should_drop_a_dependency_with_no_target() => _parsed.Dropped.Any(_ => _.Contains("no target", StringComparison.Ordinal)).ShouldBeTrue();
    [Fact] void should_drop_an_unknown_kind() => _parsed.Dropped.Any(_ => _.Contains("telepathy", StringComparison.Ordinal)).ShouldBeTrue();
}
#endif
